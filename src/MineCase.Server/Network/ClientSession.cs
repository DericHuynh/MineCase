using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using MineCase.Buffers;
using MineCase.Protocol;
using MineCase.Serialization;
using MineCase.Server.Game;
using MineCase.Server.Network;
using Orleans;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MineCase.Server.Network
{
    internal sealed class ClientSession : IDisposable
    {
        private static readonly ActivitySource _activitySource = new ActivitySource("MineCase", "1.0.0");
        private readonly TcpClient _tcpClient;
        private readonly ILogger _logger;
        private Stream _remoteStream;
        private readonly IClusterClient _grainFactory;
        private volatile bool _useCompression = false;
        private readonly Guid _sessionId;
        private readonly OutcomingPacketObserver _outcomingPacketObserver;
        private IClientboundPacketObserver _clientboundPacketObserverRef;
        private readonly ActionBlock<object> _outcomingPacketDispatcher;
        private readonly ObjectPool<UncompressedPacket> _uncompressedPacketObjectPool;
        private readonly IBufferPool<byte> _bufferPool;
        private readonly IPacketCompress _packetCompress;

        private uint _compressThreshold;

        public ClientSession(TcpClient tcpClient, ILogger<ConnectionRouter> logger, IClusterClient grainFactory, IBufferPool<byte> bufferPool, ObjectPool<UncompressedPacket> uncompressedPacketObjectPool, IPacketCompress packetCompress)
        {
            _sessionId = Guid.NewGuid();
            _tcpClient = tcpClient;
            _logger = logger;
            _grainFactory = grainFactory;
            _bufferPool = bufferPool;
            _packetCompress = packetCompress;
            _uncompressedPacketObjectPool = uncompressedPacketObjectPool;
            _outcomingPacketObserver = new OutcomingPacketObserver(this);
            _outcomingPacketDispatcher = new ActionBlock<object>(SendOutgoingPacket);
        }

        public async Task Startup(CancellationToken cancellationToken)
        {
            await using (_remoteStream = _tcpClient.GetStream())
            {
                // subscribe observer to get packet from server
                _clientboundPacketObserverRef = _grainFactory.CreateObjectReference<IClientboundPacketObserver>(_outcomingPacketObserver);
                await _grainFactory.GetGrain<IClientboundPacketSink>(_sessionId).Subscribe(_clientboundPacketObserverRef);
                try
                {
                    DateTime expiredTime = DateTime.Now + TimeSpan.FromSeconds(10);
                    while (!cancellationToken.IsCancellationRequested &&
                        !_outcomingPacketDispatcher.Completion.IsCompleted)
                    {
                        await ForwardIncomingPacket();
                        // renew subscribe, 10 sec
                        if (DateTime.Now > expiredTime)
                        {
                            await _grainFactory.GetGrain<IClientboundPacketSink>(_sessionId).Subscribe(_clientboundPacketObserverRef);
                            expiredTime = DateTime.Now + TimeSpan.FromSeconds(10);
                        }
                    }
                }
                catch (EndOfStreamException)
                {
                    var router = _grainFactory.GetGrain<IPacketRouter>(_sessionId);
                    await router.Close();

                    await _outcomingPacketDispatcher.Completion;
                }
            }
        }

        private void OnClosed()
        {
            _outcomingPacketDispatcher.Post(null);
        }

        private async Task ForwardIncomingPacket()
        {
            using var bufferScope = _bufferPool.CreateScope();
            UncompressedPacket packet;
            if (_useCompression)
            {
                var compressedPacket = await CompressedPacket.DeserializeAsync(_remoteStream, null);
                packet = _packetCompress.Decompress(compressedPacket, _compressThreshold);
            }
            else
            {
                packet = await UncompressedPacket.DeserializeAsync(_remoteStream, bufferScope);
            }

            using var activity = _activitySource.StartActivity(name: "Client Packet", ActivityKind.Server);

            if (Activity.Current is not null && Activity.Current.IsAllDataRequested)
            {
                Activity.Current.AddTag("client_packet_id", packet.PacketId);
                Activity.Current.AddTag("client_packet_length", packet.Length);
                Activity.Current.AddTag("client_packet_data", string.Join(", ", packet.Data));
            }

            await DispatchIncomingPacket(packet);
        }

        private async Task SendOutgoingPacket(object packetOrCommand)
        {
            
            switch (packetOrCommand)
            {
                case null:
                    Activity.Current?.AddTag("packet_data", "null");
                    _logger.LogDebug("Null Packet for client {remote}", _tcpClient.Client.RemoteEndPoint.ToString());
                    _tcpClient.Client.Shutdown(SocketShutdown.Send);
                    _outcomingPacketDispatcher.Complete();
                    break;
                case UncompressedPacket packet:
                {
                    using var bufferScope = _bufferPool.CreateScope();

                    if(Activity.Current is not null && Activity.Current.IsAllDataRequested)
                    {
                        Activity.Current?.AddTag("server_packet_id", packet.PacketId);
                        Activity.Current?.AddTag("server_packet_length", packet.Length);
                        Activity.Current?.AddTag("server_packet_data", string.Join(", ", packet.Data));
                    }

                    if (_useCompression)
                    {
                        var newPacket = _packetCompress.Compress(packet, _compressThreshold);
                        await newPacket.SerializeAsync(_remoteStream);
                    }
                    else
                    {
                        await packet.SerializeAsync(_remoteStream);
                    }

                    if (!_useCompression && packet.PacketId == Protocol.Protocol.SetCompressionPacketId)
                    {
                        _compressThreshold = GetCompressionThreshold(packet);
                        _useCompression = true;
                    }

                    break;
                }
                default:
                    Activity.Current?.AddTag("packet_data", "Unknown (Not null or Uncompressed packet)");
                    break;

            }
        }

        private static uint GetCompressionThreshold(UncompressedPacket packet)
        {
            var br = new SpanReader(packet.Data);
            return br.ReadAsVarInt(out _);
        }

        private async Task DispatchIncomingPacket(UncompressedPacket packet)
        {
            var router = _grainFactory.GetGrain<IPacketRouter>(_sessionId);
            await router.SendPacket(packet);
        }

        private async void DispatchOutcomingPacket(object packet)
        {
            try
            {
                if (!_outcomingPacketDispatcher.Completion.IsCompleted)
                    await _outcomingPacketDispatcher.SendAsync(packet);
            }
            catch
            {
                _outcomingPacketDispatcher.Complete();
            }
        }

        private class OutcomingPacketObserver(ClientSession session) : IClientboundPacketObserver
        {
            public void OnClosed()
            {
                session.OnClosed();
            }

            public void ReceivePacket(UncompressedPacket packet)
            {
                session.DispatchOutcomingPacket(packet);
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false; // 要检测冗余调用

        private void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                _tcpClient.Dispose();
            }
            _disposedValue = true;
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
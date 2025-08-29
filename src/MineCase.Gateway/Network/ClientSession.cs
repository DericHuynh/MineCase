using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using MineCase.Buffers;
using MineCase.Protocol;
using MineCase.Serialization;
using MineCase.Server;
using MineCase.Server.Network;
using Orleans;
using Orleans.Streams;

namespace MineCase.Gateway.Network
{
    class ClientSession : IDisposable
    {
        #region Dependency Injection
        private readonly IClusterClient _grainFactory;
        private readonly ILogger<ClientSession> _logger;
        private readonly IPacketCompress _packetCompress;
        private readonly ObjectPool<UncompressedPacket> _uncompressedPacketObjectPool;
        private readonly IBufferPool<byte> _bufferPool;
        private readonly TcpClient _tcpClient;
        #endregion

        private volatile bool _useCompression = false;
        private int _compressThreshold;

        private readonly Guid _sessionId;
        private readonly ActionBlock<UncompressedPacket> _outcomingPacketDispatcher;
        private Stream _minecraftClientStream;

        // Streams for bi-directional communication with the silo
        private IAsyncStream<UncompressedPacket> _incomingStream; // To the silo
        private IAsyncStream<UncompressedPacket> _outgoingStream; // From the silo
        private StreamSubscriptionHandle<UncompressedPacket> _outgoingStreamSubscription;

        public ClientSession(TcpClient tcpClient, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, IClusterClient grainFactory, IBufferPool<byte> bufferPool, ObjectPool<UncompressedPacket> uncompressedPacketObjectPool, IPacketCompress packetCompress)
        {
            _sessionId = Guid.NewGuid();
            _tcpClient = tcpClient;
            _logger = loggerFactory.CreateLogger<ClientSession>();
            _grainFactory = grainFactory;
            _bufferPool = bufferPool;
            _packetCompress = packetCompress;
            _uncompressedPacketObjectPool = uncompressedPacketObjectPool;

            _outcomingPacketDispatcher = new ActionBlock<UncompressedPacket>(async packet =>
            {
                if (packet == null)
                {
                    if (_tcpClient.Connected)
                        _tcpClient.Client.Shutdown(SocketShutdown.Send);
                }
                else
                {
                    await SendPacketToClient(packet);
                }
            });
        }

        public async Task Startup(CancellationToken cancellationToken)
        {
            using (_minecraftClientStream = _tcpClient.GetStream())
            {
                var streamProvider = _grainFactory.GetStreamProvider(StreamProviders.MinecraftStreamProvider);
                var router = _grainFactory.GetGrain<IPacketRouter>(_sessionId);

                // 1. Set up the stream for packets FROM the silo TO this client.
                _outgoingStream = streamProvider.GetStream<UncompressedPacket>("session-stream", _sessionId);
                _outgoingStreamSubscription = await _outgoingStream.SubscribeAsync(
                    (packet, token) =>
                    {
                        DispatchOutcomingPacket(packet);
                        return Task.CompletedTask;
                    });

                // 2. Set up the stream for packets FROM this client TO the silo.
                _incomingStream = streamProvider.GetStream<UncompressedPacket>("client-incoming", _sessionId);

                // 3. Tell the router grain which stream to use to send packets back to us.
                await router.SetClientStream(_outgoingStream.StreamId);

                try
                {
                    while (!cancellationToken.IsCancellationRequested && !_outcomingPacketDispatcher.Completion.IsCompleted && _tcpClient.Connected)
                    {
                        await ReadAndPublishIncomingPacket();
                    }
                }
                catch (Exception ex) when (ex is EndOfStreamException || ex is IOException)
                {
                    _logger.LogInformation("Client {SessionId} disconnected: {ExceptionMessage}", _sessionId, ex.Message);
                }
                finally
                {
                    await router.Close();

                    _outcomingPacketDispatcher.Post(null); // Signal the sender to shut down.
                    await _outcomingPacketDispatcher.Completion;

                    if (_incomingStream != null)
                    {
                        // Signal that this client will not be sending any more packets.
                        await _incomingStream.OnCompletedAsync();
                    }

                    if (_outgoingStreamSubscription != null)
                    {
                        await _outgoingStreamSubscription.UnsubscribeAsync();
                    }
                }
            }
        }

        private async Task ReadAndPublishIncomingPacket()
        {
            using (var bufferScope = _bufferPool.CreateScope())
            {
                UncompressedPacket packet;
                if (_useCompression)
                {
                    var compressedPacket = await CompressedPacket.DeserializeAsync(_minecraftClientStream, null);
                    packet = _packetCompress.Decompress(compressedPacket, _compressThreshold);
                }
                else
                {
                    packet = await UncompressedPacket.DeserializeAsync(_minecraftClientStream, bufferScope);
                }

                // Publish the packet to the stream. The implicitly subscribed PacketRouterGrain will receive it.
                await _incomingStream.OnNextAsync(packet);
            }
        }

        private async Task SendPacketToClient(UncompressedPacket packet)
        {
            try
            {
                using (var bufferScope = _bufferPool.CreateScope())
                {
                    if (_useCompression)
                    {
                        var newPacket = _packetCompress.Compress(packet, _compressThreshold);
                        await newPacket.SerializeAsync(_minecraftClientStream);
                    }
                    else
                    {
                        await packet.SerializeAsync(_minecraftClientStream);
                    }

                    // Hardcoded packet ID, consider using a constant from your protocol definition.
                    if (!_useCompression && packet.PacketId == 0x03) // Protocol.Protocol.SetCompressionPacketId
                    {
                        _compressThreshold = GetCompressionThreshold(packet);
                        _useCompression = true;
                        _logger.LogDebug("Compression enabled for session {SessionId} with threshold {Threshold}", _sessionId, _compressThreshold);
                    }
                }
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "Failed to send packet to client {SessionId}, connection may be closed.", _sessionId);
                _outcomingPacketDispatcher.Complete();
            }
        }

        private static int GetCompressionThreshold(UncompressedPacket packet)
        {
            var br = new SpanReader(packet.Data);
            return br.ReadAsVarInt(out _);
        }

        private void DispatchOutcomingPacket(UncompressedPacket packet)
        {
            if (!_outcomingPacketDispatcher.Post(packet))
            {
                _logger.LogWarning("Could not post packet to dispatcher for session {SessionId}. It may be shutting down.", _sessionId);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _tcpClient.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
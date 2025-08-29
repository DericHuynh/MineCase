using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MineCase.Protocol;
using MineCase.Server.User;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Streams;

namespace MineCase.Server.Network
{
    /// <summary>
    /// Packet router grain for a session. It implicitly subscribes to a stream of packets from the client.
    /// </summary>
    [ImplicitStreamSubscription("client-incoming")]
    [Reentrant]
    internal partial class PacketRouterGrain : Grain, IPacketRouter, IAsyncObserver<UncompressedPacket>, IClientboundPacketSink
    {
        private readonly ILogger _logger;
        private readonly IPacketPackager _packetPackager;

        private SessionState _state = SessionState.Handshaking;
        private int _protocolVersion;
        private string _userName;
        private IUser _user;

        private IAsyncStream<UncompressedPacket> _clientboundStream; // The stream for sending packets back to the client
        private StreamSubscriptionHandle<UncompressedPacket> _incomingStreamHandle;

        public PacketRouterGrain(ILoggerFactory loggerFactory, IPacketPackager packetPackager)
        {
            _logger = loggerFactory.CreateLogger<PacketRouterGrain>();
            _packetPackager = packetPackager;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider(StreamProviders.MinecraftStreamProvider);
            var streamId = this.GetPrimaryKey();

            var stream = streamProvider.GetStream<UncompressedPacket>("client-incoming", streamId);
            _incomingStreamHandle = await stream.SubscribeAsync(this);

            _clientboundStream = streamProvider.GetStream<UncompressedPacket>("session-stream", streamId);

            _logger.LogDebug("PacketRouterGrain {SessionId} activated.", streamId);

            await base.OnActivateAsync(cancellationToken);
        }

        /// <summary>
        /// This is the entry point for all packets coming from the client, delivered by the stream.
        /// </summary>
        /// <param name="packet">The uncompressed packet to inspect and handle.</param>
        /// <param name="token">Stream sequence token for order.</param>
        /// <returns>Completed Task.</returns>
        public Task OnNextAsync(UncompressedPacket packet, StreamSequenceToken token = null)
        {
            ArgumentNullException.ThrowIfNull(packet);
            ArgumentNullException.ThrowIfNull(packet);
            using var activity = ActivitySources.NetworkActivitySource.StartActivity("Process Packet from Stream");
            Activity.Current?.SetTag("SessionState", Enum.GetName(typeof(SessionState), _state));
            Activity.Current?.SetTag("PacketId", $"0x{packet.PacketId:X2}");

            switch (_state)
            {
                case SessionState.Handshaking:
                    _logger.LogDebug("Handshaking Packet: id = {id}", packet.PacketId);
                    return DispatchHandshakingPackets(packet);
                case SessionState.Status:
                    _logger.LogDebug("Status Packet: id = {id}", packet.PacketId);
                    return DispatchStatusPackets(packet);
                case SessionState.Login:
                    _logger.LogDebug("Login Packet: id = {id}", packet.PacketId);
                    return DispatchLoginPackets(packet);
                case SessionState.Configuration:
                    _logger.LogDebug("Configuration Packet: id = {id}", packet.PacketId);
                    return DispatchConfigurationPackets(packet);
                case SessionState.Play:
                    _logger.LogDebug("Play Packet: id = {id}", packet.PacketId);
                    return _user.ForwardPacket(packet);
                case SessionState.Closed:
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }

        public Task OnCompletedAsync()
        {
            _logger.LogInformation("Client {SessionId} has completed its incoming stream.", this.GetPrimaryKey());

            // The client has gracefully disconnected, so we'll close our end too.
            return Close();
        }

        public Task OnErrorAsync(Exception ex)
        {
            _logger.LogError(ex, "An error occurred on the incoming stream for session {SessionId}.", this.GetPrimaryKey());
            return Close();
        }

        public async Task Close()
        {
            using var close_activity = ActivitySources.NetworkActivitySource.StartActivity("Close PacketRouter", ActivityKind.Client, parentId: Activity.Current?.ParentId);

            if (_user != null)
                await _user.Kick();

            _state = SessionState.Closed;
            _logger.LogDebug("State Changed to {state}", _state);

            if (_incomingStreamHandle != null)
            {
                await _incomingStreamHandle.UnsubscribeAsync();
            }

            if (_clientboundStream != null)
            {
                // Signal to the client that we won't be sending any more packets.
                await _clientboundStream.OnCompletedAsync();
            }

            DeactivateOnIdle();
        }

        public Task SetClientStream(StreamId streamId)
        {
            var streamProvider = this.GetStreamProvider(StreamProviders.MinecraftStreamProvider);
            _clientboundStream = streamProvider.GetStream<UncompressedPacket>(streamId);
            _logger.LogDebug("Clientbound stream set for session {SessionId}", this.GetPrimaryKey());
            return Task.CompletedTask;
        }

        public Task Configuration()
        {
            _state = SessionState.Configuration;
            _logger.LogDebug("State Changed to {state}", _state);
            return Task.CompletedTask;
        }

        public Task Play()
        {
            _state = SessionState.Play;
            _logger.LogDebug("State Changed to {state}", _state);
            return Task.CompletedTask;
        }

        public Task BindToUser(IUser user)
        {
            _user = user;
            return Task.CompletedTask;
        }

        public Task<IUser> GetUser() => Task.FromResult(_user);

        public Task SetUserName(string name)
        {
            _userName = name;
            return Task.CompletedTask;
        }

        public Task<string> GetUserName() => Task.FromResult(_userName);

        public async Task SendPacket(IPacket packet)
        {
            var prepared = await _packetPackager.PreparePacket(packet);
            await SendPacket(prepared.PacketId, prepared.Data.AsImmutable());
        }

        public Task SendPacket(int packetId, Immutable<byte[]> data)
        {
            var packet = new UncompressedPacket
            {
                PacketId = packetId,
                Data = new ArraySegment<byte>(data.Value)
            };

            _logger.LogDebug("Sending packet via stream: id=0x{PacketId:X2}, session={SessionId}", packetId, this.GetPrimaryKey());
            return _clientboundStream.OnNextAsync(packet);
        }

        // No-op methods for backward compatibility.
        public Task Subscribe(IClientboundPacketObserver observer) => Task.CompletedTask;

        public Task UnSubscribe(IClientboundPacketObserver observer) => Task.CompletedTask;

        public enum SessionState
        {
            /// <summary> Hand shaking stage is the first step of client login.</summary>
            Handshaking,

            /// <summary> Clients ping and get server motd before login.</summary>
            Status,

            /// <summary> Seems to be Client configuration before joining server. </summary>
            Configuration,

            /// <summary> login stage.</summary>
            Login,

            /// <summary> Session switches to PlayState when players are playing game.</summary>
            Play,

            /// <summary> Clients Disconnection.</summary>
            Closed
        }
    }
}
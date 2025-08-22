using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MineCase.Protocol;
using MineCase.Protocol.Handshaking;
using MineCase.Server.Settings;
using MineCase.Server.User;
using Orleans;
using Orleans.Concurrency;

namespace MineCase.Server.Network
{
    /// <summary>
    /// Packet router grain. It send different packet to partial class by its session state.
    /// </summary>
    [Reentrant]
    internal partial class PacketRouterGrain : Grain, IPacketRouter
    {
        private SessionState _state = SessionState.Handshaking;
        private uint _protocolVersion;
        private string _userName;
        private IUser _user;
        private readonly ILogger _logger;

        public PacketRouterGrain(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PacketRouterGrain>();
        }

        public Task SendPacket(UncompressedPacket packet)
        {
            _logger.LogInformation("Packet: id = {id}, length = {length}; Data = {data}", packet.PacketId, packet.Length, string.Join(", ", packet.Data));

            switch (_state)
            {
                case SessionState.Handshaking:
                    return DispatchHandshakingPackets(packet);
                case SessionState.Status:
                    return DispatchStatusPackets(packet);
                case SessionState.Login:
                    return DispatchLoginPackets(packet);
                case SessionState.Play:
                    return _user.ForwardPacket(packet);
                case SessionState.Closed:
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }

        public async Task Close()
        {
            if (_user != null)
                await _user.Kick();
            _state = SessionState.Closed;
            _logger.LogInformation("State Changed to {state}", _state);
            await GrainFactory.GetGrain<IClientboundPacketSink>(this.GetPrimaryKey()).Close();
            DeactivateOnIdle();
        }

        public Task Play()
        {
            _state = SessionState.Play;
            _logger.LogInformation("State Changed to {state}", _state);
            return Task.CompletedTask;
        }

        public Task BindToUser(IUser user)
        {
            _user = user;
            return Task.CompletedTask;
        }

        public Task<IUser> GetUser()
        {
            return Task.FromResult(_user);
        }

        public Task SetUserName(string name)
        {
            _userName = name;
            return Task.CompletedTask;
        }

        public Task<string> GetUserName()
        {
            return Task.FromResult(_userName);
        }

        private sealed class DeferredPacketMark
        {
        }

        public enum SessionState
        {
            /// <summary> Hand shaking stage is the first step of client login.</summary>
            Handshaking,

            /// <summary> Clients ping and get server motd before login.</summary>
            Status,

            /// <summary> login stage.</summary>
            Login,

            /// <summary> Session switches to PlayState when players are playing game.</summary>
            Play,

            /// <summary> Clients Disconnection.</summary>
            Closed
        }
    }
}

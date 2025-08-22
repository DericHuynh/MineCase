using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MineCase.Protocol;
using MineCase.Protocol.Configuration;
using MineCase.Protocol.Login;
using MineCase.Protocol.Status;
using MineCase.Serialization;
using MineCase.Server.Network.Login;
using MineCase.Server.Network.Status;
using MineCase.Server.User;
using Orleans;

namespace MineCase.Server.Network
{
    /// <summary>
    /// Packet router grain used in status requests and response.
    /// </summary>
    internal partial class PacketRouterGrain
    {
        private Task DispatchConfigurationPackets(UncompressedPacket packet)
        {
            var br = new SpanReader(packet.Data);
            switch (packet.PacketId)
            {
                // Client Information
                case 0x00:
                    return DispatchPacket(PacketDeserializer.Deserialize<ServerBoundClientInformation>(ref br));
                default:
                    throw new InvalidDataException($"Configuration State - Unrecognizable packet id: 0x{packet.PacketId:X2}.");
            }
        }

        private Task DispatchPacket(ServerBoundClientInformation packet)
        {
            _state = SessionState.Play;

            // TODO
            return Task.CompletedTask;
        }
    }
}

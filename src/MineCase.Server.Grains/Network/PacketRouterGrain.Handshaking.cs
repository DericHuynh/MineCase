using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MineCase.Protocol;
using MineCase.Protocol.Handshaking;
using MineCase.Serialization;
using MineCase.Server.Network.Handshaking;
using Orleans;

namespace MineCase.Server.Network
{
    /// <summary>
    /// Packet router grain used in handshaking stage.
    /// </summary>
    internal partial class PacketRouterGrain
    {
        private Task DispatchHandshakingPackets(UncompressedPacket packet)
        {
            var br = new SpanReader(packet.Data);
            switch (packet.PacketId)
            {
                // Handshake
                case 0x00:
                    return DispatchPacket(PacketDeserializer.Deserialize<Handshake>(ref br));
                default:
                    throw new InvalidDataException($"Handshaking State - Unrecognizable packet id: 0x{packet.PacketId:X2}.");
            }
        }

        private Task DispatchPacket(Handshake packet)
        {
            using var activity = ActivitySources.NetworkActivitySource.StartActivity("Handle Handshake", ActivityKind.Server, parentId: Activity.Current?.ParentId);
            Activity.Current?.SetTag("ClientPacketType", nameof(Handshake));
            Activity.Current?.SetTag("ClientProtocolVersion", packet.ProtocolVersion);
            Activity.Current?.SetTag("ClientServerAddress", packet.ServerAddress);
            Activity.Current?.SetTag("ClientServerPort", packet.ServerPort);
            Activity.Current?.SetTag("ClientNextState", packet.NextState);

            if (packet.NextState == 1)
                _state = SessionState.Status;
            else if (packet.NextState == 2)
                _state = SessionState.Login;
            else
                throw new InvalidOperationException();

            _protocolVersion = packet.ProtocolVersion;
            return Task.CompletedTask;
        }
    }
}

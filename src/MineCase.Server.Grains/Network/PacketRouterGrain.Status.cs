using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MineCase.Protocol;
using MineCase.Protocol.Handshaking;
using MineCase.Protocol.Status;
using MineCase.Serialization;
using MineCase.Server.Network.Status;
using Orleans;

namespace MineCase.Server.Network
{
    /// <summary>
    /// Packet router grain used in status requests and response.
    /// </summary>
    internal partial class PacketRouterGrain
    {
        private Task DispatchStatusPackets(UncompressedPacket packet)
        {
            var br = new SpanReader(packet.Data);
            using var activity = ActivitySources.NetworkActivitySource.StartActivity("Status Packet", ActivityKind.Server, parentId: Activity.Current?.ParentId);
            activity.AddTag("PacketId", packet.PacketId);
            activity.AddTag("Length", packet.Length);
            activity.AddTag("Data", packet.Data != null ? string.Join(", ", packet.Data) : "");

            switch (packet.PacketId)
            {
                // Request
                case 0x00:
                    return DispatchPacket(PacketDeserializer.Deserialize<Request>(ref br));

                // Ping
                case 0x01:
                    return DispatchPacket(PacketDeserializer.Deserialize<Ping>(ref br));
                default:
                    throw new InvalidDataException($"Status State - Unrecognizable packet id: 0x{packet.PacketId:X2}.");
            }
        }

        private Task DispatchPacket(Request packet)
        {
            var requestGrain = GrainFactory.GetGrain<IRequest>(0);
            requestGrain.DispatchPacket(this.GetPrimaryKey(), packet).Ignore();
            return Task.CompletedTask;
        }

        private Task DispatchPacket(Ping packet)
        {
            var requestGrain = GrainFactory.GetGrain<IPing>(0);
            requestGrain.DispatchPacket(this.GetPrimaryKey(), packet).Ignore();
            return Task.CompletedTask;
        }
    }
}

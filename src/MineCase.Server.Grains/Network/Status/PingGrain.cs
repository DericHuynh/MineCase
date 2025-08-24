using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MineCase.Protocol.Status;
using Orleans;
using Orleans.Concurrency;

namespace MineCase.Server.Network.Status
{
    [StatelessWorker]
    [Reentrant]
    internal class PingGrain : Grain, IPing
    {
        public async Task DispatchPacket(Guid sessionId, Ping packet)
        {
            using var ping_activity = ActivitySources.NetworkActivitySource.StartActivity("Ping", ActivityKind.Client, parentId: Activity.Current?.ParentId);
            if (Activity.Current is not null && Activity.Current.IsAllDataRequested)
            {
                Activity.Current.DisplayName = "Handle " + nameof(Ping);
                Activity.Current.AddTag("Payload", packet.Payload);
            }

            using var pong_activity = ActivitySources.NetworkActivitySource.StartActivity("Send Pong", ActivityKind.Client, parentId: Activity.Current?.ParentId);
            Pong pongPacket = new Pong
            {
                Payload = packet.Payload
            };

            if (Activity.Current is not null && Activity.Current.IsAllDataRequested)
            {
                Activity.Current.DisplayName = "Send " + nameof(Pong);
                Activity.Current.AddTag("Payload", pongPacket.Payload);
            }

            await GrainFactory.GetGrain<IClientboundPacketSink>(sessionId).SendPacket(pongPacket);

            GrainFactory.GetGrain<IPacketRouter>(sessionId).Close().Ignore();
        }
    }
}

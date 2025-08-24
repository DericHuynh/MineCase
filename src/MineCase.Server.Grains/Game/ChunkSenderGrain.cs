using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Server.Network;
using MineCase.Server.User;
using MineCase.Server.World;
using MineCase.World;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;

namespace MineCase.Server.Game
{
    [Reentrant]
    internal class ChunkSenderGrain : Grain, IChunkSender
    {
        private Guid _jobWorkerId;

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _jobWorkerId = Guid.NewGuid();
            return base.OnActivateAsync(cancellationToken);
        }

        public Task PostChunk(ChunkWorldPos chunkPos, IReadOnlyCollection<IClientboundPacketSink> clients, IReadOnlyCollection<IUserChunkLoader> loaders)
        {
            StreamId streamId = StreamId.Create(StreamProviders.Namespaces.ChunkSender, _jobWorkerId);
            var stream = this.GetStreamProvider(StreamProviders.JobsProvider).GetStream<SendChunkJob>(streamId);
            return stream.OnNextAsync(new SendChunkJob
            {
                World = GrainFactory.GetGrain<IWorld>(this.GetPrimaryKeyString()),
                ChunkPosition = chunkPos,
                Clients = clients,
                Loaders = loaders
            });
        }
    }
}

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
using Orleans.Streams;

namespace MineCase.Server.Game
{
    /// <summary>
    /// GrainID should be world primary key, such as "default", "nether", "end", etc.
    /// This creates a chunk sender "queuer" for each silo for each world type.
    /// This may later have an accumulator to accumulate many messages before sending chunks in batches to many clients/loaders.
    /// Or removed entirely.
    ///
    /// Honestly this should all be removed and replaced with a stream directly to the Orleans Client, with the StreamID being something like their SessionId.
    /// </summary>
    [Reentrant]
    internal class ChunkSenderGrain : Grain, IChunkSender
    {
        private IWorld world;

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            world = GrainFactory.GetGrain<IWorld>(this.GetPrimaryKeyString());
            return base.OnActivateAsync(cancellationToken);
        }

        public Task PostChunk(ChunkWorldPos chunkPos, IReadOnlyCollection<IClientboundPacketSink> clients, IReadOnlyCollection<IUserChunkLoader> loaders)
        {
            IStreamProvider streamProvider = this.GetStreamProvider(StreamProviders.MinecraftStreamProvider);
            StreamId streamId = StreamId.Create(StreamProviders.Namespaces.ChunkSender, world.GetPrimaryKeyString());
            IAsyncStream<SendChunkJob> stream = streamProvider.GetStream<SendChunkJob>(streamId);
            return stream.OnNextAsync(new SendChunkJob
            {
                World = world,
                ChunkPosition = chunkPos,
                Clients = clients,
                Loaders = loaders
            });
        }
    }
}

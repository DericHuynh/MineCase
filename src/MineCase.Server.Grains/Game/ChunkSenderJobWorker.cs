using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MineCase.Protocol;
using MineCase.Server.Network;
using MineCase.Server.Network.Play;
using MineCase.Server.User;
using MineCase.Server.World;
using MineCase.World;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Streams;

namespace MineCase.Server.Game
{
    [Orleans.GenerateSerializer]
    public sealed class SendChunkJob
    {
        [Id(0)]
        public IWorld World { get; set; }

        [Id(1)]
        public ChunkWorldPos ChunkPosition { get; set; }

        [Id(2)]
        public IReadOnlyCollection<IClientboundPacketSink> Clients { get; set; }

        [Id(3)]
        public IReadOnlyCollection<IUserChunkLoader> Loaders { get; set; }
    }

    internal interface IChunkSenderJobWorker : IGrainWithStringKey
    {
    }

    [ImplicitStreamSubscription(StreamProviders.Namespaces.ChunkSender)]
    [Reentrant]
    internal class ChunkSenderJobWorker : Grain, IChunkSenderJobWorker
    {
        /// <summary>
        /// Why the fuck are you using different jobId's per reactivation (like if the chunk is unloaded) if you want to recycle the streams?
        /// Makes no sense.
        /// </summary>
        private readonly IPacketPackager _packetPackager;

        public ChunkSenderJobWorker(IPacketPackager packetPackager)
        {
            _packetPackager = packetPackager;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider(StreamProviders.MinecraftStreamProvider);
            var streamId = StreamId.Create(StreamProviders.Namespaces.ChunkSender, this.GetPrimaryKeyString());
            var stream = streamProvider.GetStream<SendChunkJob>(streamId);
            await stream.SubscribeAsync(OnNextAsync);
        }

        private async Task OnNextAsync(SendChunkJob job, StreamSequenceToken token)
        {
            var chunkColumn = GrainFactory.GetGrain<IChunkColumn>(job.World.MakeAddressByPartitionKey(job.ChunkPosition));
            var generator = new ClientPlayPacketFactory(new BroadcastPacketSink(job.Clients, _packetPackager));
            var chunkColumnStorage = await chunkColumn.GetState();
            await generator.ChunkData(Dimension.Overworld, job.ChunkPosition.X, job.ChunkPosition.Z, chunkColumnStorage);

            // I think you always need to send light updates, which is why newest version packets are "Send Chunk and Light" instead of just chunks. Update for 1.21.8
            await generator.LightUpdate(Dimension.Overworld, job.ChunkPosition.X, job.ChunkPosition.Z, chunkColumnStorage);
            foreach (var loader in job.Loaders)
                loader.OnChunkSent(job.ChunkPosition).Ignore();
        }
    }
}

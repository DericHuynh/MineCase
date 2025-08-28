using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MineCase.Server.Game;
using MineCase.Server.Game.Entities;
using MineCase.Server.Network;
using MineCase.Server.Network.Play;
using MineCase.Server.Settings;
using MineCase.Server.World;
using MineCase.World;
using Orleans;
using Orleans.Concurrency;
using Orleans.Providers;
using Orleans.Runtime;

namespace MineCase.Server.User
{
    [GenerateSerializer]
    public class UserChunkLoaderState
    {
        // Persisted grain/entity IDs
        [Id(0)]
        public string WorldId { get; set; }

        [Id(1)]
        public string PlayerId { get; set; }

        // Persisted observer and dependent generator
        [Id(2)]
        public string SinkId { get; set; }

        // Persisted chunk management state
        [Id(3)]
        public HashSet<ChunkWorldPos> SendingChunks { get; set; } = new HashSet<ChunkWorldPos>();

        [Id(4)]
        public HashSet<ChunkWorldPos> SentChunks { get; set; } = new HashSet<ChunkWorldPos>();

        [Id(5)]
        public ChunkWorldPos? LastStreamedChunk { get; set; }

        // Persisted settings
        [Id(6)]
        public int ViewDistance { get; set; } = 10;
    }

    [Reentrant]
    [StorageProvider(ProviderName = "Default")]
    internal class UserChunkLoaderGrain : Grain, IUserChunkLoader
    {
        private readonly IPersistentState<UserChunkLoaderState> _persistence;
        private readonly ILogger<UserChunkLoaderGrain> _logger;

        // --- TRANSIENT STATE ---
        // Only live grain references that can be re-acquired from persisted IDs are transient.
        // Everything else, including observers and stateful helpers, is now in _persistence.State.
        private IPlayer _player;
        private IWorld _world;
        private IClientboundPacketSink _sink;
        private ClientPlayPacketFactory _generator;

        // Helper property to check readiness.
        private bool IsFullyInitialized => _player != null && _world != null;

        public UserChunkLoaderGrain(
            [PersistentState("userChunkLoader")] IPersistentState<UserChunkLoaderState> persistence,
            ILogger<UserChunkLoaderGrain> logger)
        {
            _persistence = persistence;
            _logger = logger;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await _persistence.ReadStateAsync();

            // Re-hydrate transient grain references from persisted IDs.
            if (!string.IsNullOrEmpty(_persistence.State.WorldId))
            {
                _world = GrainFactory.GetGrain<IWorld>(_persistence.State.WorldId);
                _player = GrainFactory.GetGrain<IPlayer>(Guid.Parse(_persistence.State.PlayerId));
            }

            if (!string.IsNullOrEmpty(_persistence.State.SinkId))
            {
                _sink = GrainFactory.GetGrain<IClientboundPacketSink>(Guid.Parse(_persistence.State.SinkId));
                _generator = new ClientPlayPacketFactory(_sink);
            }

            await base.OnActivateAsync(cancellationToken);
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            _persistence.State.WorldId = _world.GetPrimaryKeyString();
            _persistence.State.PlayerId = _player.GetPrimaryKey().ToString();
            _persistence.State.SinkId = _sink.GetPrimaryKey().ToString();

            await _persistence.WriteStateAsync();

            await base.OnDeactivateAsync(reason, cancellationToken);
        }

        public async Task OnGameTick(GameTickArgs e, EntityWorldPos playerPosition)
        {
            if (!IsFullyInitialized) return;

            if (e.WorldAge % 10 == 0)
            {
                // Limit the number of chunks being sent simultaneously.
                int maxConcurrentSends = 4;
                for (int i = 0; i < maxConcurrentSends && _persistence.State.SendingChunks.Count <= maxConcurrentSends; i++)
                {
                    if (await StreamNextChunk(playerPosition.ToChunkWorldPos())) break;
                }
            }

            if (e.WorldAge % 100 == 0)
            {
                await UnloadOutOfRangeChunks();
            }
        }

        public Task OnChunkSent(ChunkWorldPos chunkPos)
        {
            // A chunk has been successfully sent. Move it from the 'Sending' set to the 'Sent' set.
            _persistence.State.SendingChunks.Remove(chunkPos);
            _persistence.State.SentChunks.Add(chunkPos);
            return Task.CompletedTask;
        }

        private async Task<bool> StreamNextChunk(ChunkWorldPos currentChunk)
        {
            if (_persistence.State.LastStreamedChunk.HasValue && _persistence.State.LastStreamedChunk.Value == currentChunk)
                return true;

            for (int d = 0; d <= _persistence.State.ViewDistance; d++)
            {
                for (int x = -d; x <= d; x++)
                {
                    var z = d - Math.Abs(x);
                    if (await StreamChunk(new ChunkWorldPos(currentChunk.X + x, currentChunk.Z + z))) return false;
                    if (z != 0 && await StreamChunk(new ChunkWorldPos(currentChunk.X + x, currentChunk.Z - z))) return false;
                }
            }

            _persistence.State.LastStreamedChunk = currentChunk;
            return true;
        }

        private async Task<bool> StreamChunk(ChunkWorldPos chunkPos, bool isRecovery = false)
        {
            // A chunk should only be streamed if it's not already sent and not currently being sent.
            if (!_persistence.State.SentChunks.Contains(chunkPos) && !_persistence.State.SendingChunks.Contains(chunkPos))
            {
                // "Write-Ahead": First, mark the chunk as 'Sending' and persist this intent.
                // This ensures that if we crash before the call completes, we can recover on next activation.
                _persistence.State.SendingChunks.Add(chunkPos);
                var chunkSender = GrainFactory.GetGrain<IChunkSender>(_world.GetPrimaryKeyString());
                await chunkSender.PostChunk(chunkPos, new[] { _sink }, new[] { this.AsReference<IUserChunkLoader>() });
                await GrainFactory.GetPartitionGrain<IChunkTrackingHub>(_world, chunkPos).Subscribe(_player);
                await GrainFactory.GetPartitionGrain<IWorldPartition>(_world, chunkPos).Enter(_player);
                return true;
            }

            return false;
        }

        private async Task UnloadOutOfRangeChunks()
        {
            if (!IsFullyInitialized) return;

            var currentChunk = (await _player.GetPosition()).ToChunkWorldPos();
            var chunksToUnload = new List<ChunkWorldPos>();

            foreach (var chunkPos in _persistence.State.SentChunks)
            {
                int distance = Math.Abs(chunkPos.X - currentChunk.X) + Math.Abs(chunkPos.Z - currentChunk.Z);
                if (distance > _persistence.State.ViewDistance)
                {
                    await GrainFactory.GetPartitionGrain<IChunkTrackingHub>(_world, chunkPos).Unsubscribe(_player);
                    await GrainFactory.GetPartitionGrain<IWorldPartition>(_world, chunkPos).Leave(_player);
                    await _generator.UnloadChunk(chunkPos.X, chunkPos.Z);
                    chunksToUnload.Add(chunkPos);
                }
            }

            if (chunksToUnload.Count > 0)
            {
                foreach (var chunkPos in chunksToUnload)
                {
                    _persistence.State.SentChunks.Remove(chunkPos);
                }
            }
        }

        public Task SetClientPacketSink(IClientboundPacketSink sink)
        {
            // This is the entry point for configuring the grain with the client connection info.
            _sink = sink;
            _persistence.State.SinkId = _sink.GetPrimaryKey().ToString();
            _generator = new ClientPlayPacketFactory(sink);

            return Task.CompletedTask;
        }

        public Task JoinGame(IWorld world, IPlayer player)
        {
            _world = world;
            _player = player;

            // Perform a full state reset for a new game session.
            _persistence.State.WorldId = world.GetPrimaryKeyString();
            _persistence.State.PlayerId = player.GetPrimaryKey().ToString();
            _persistence.State.SendingChunks.Clear();
            _persistence.State.SentChunks.Clear();
            _persistence.State.LastStreamedChunk = null;

            // The Sink and Generator are kept, as per the requirement that they represent the persistent client connection.
            // If a new sink is needed on JoinGame, the caller should call SetClientPacketSink again.
            return Task.CompletedTask;
        }

        public async Task SetViewDistance(int viewDistance)
        {
            var settings = GrainFactory.GetGrain<IServerSettings>(0);
            var maxViewDistance = (await settings.GetSettings()).ViewDistance;

            int newViewDistance = (viewDistance >= 2 && viewDistance <= maxViewDistance)
                ? viewDistance
                : (int)maxViewDistance;

            if (newViewDistance != _persistence.State.ViewDistance)
            {
                _persistence.State.ViewDistance = newViewDistance;
                _persistence.State.LastStreamedChunk = null; // Force rescan
            }
        }
    }
}

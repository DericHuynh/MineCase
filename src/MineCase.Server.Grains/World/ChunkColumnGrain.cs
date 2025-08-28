using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MineCase.Block;
using MineCase.Server.Game.BlockEntities;
using MineCase.Server.Game.Blocks;
using MineCase.Server.Game.Entities;
using MineCase.Server.Network.Play;
using MineCase.Server.Persistence;
using MineCase.Server.Persistence.Components;
using MineCase.Server.Settings;
using MineCase.Server.World.Generation;
using MineCase.World;
using MineCase.World.Biomes;
using MineCase.World.Generation;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Orleans;
using Orleans.Concurrency;

namespace MineCase.Server.World
{
    [Reentrant]
    [PersistTableName("chunkColumn")]
    internal class ChunkColumnGrain : AddressByPartitionGrain, IChunkColumn
    {
        private StateHolder State => GetValue(StateComponent<StateHolder>.StateProperty);

        private Task _generationTask;

        private Task _populationTask;

        protected override void InitializePreLoadComponent()
        {
            SetComponent(new StateComponent<StateHolder>());
        }

        protected override void InitializeComponents()
        {
            SetComponent(new PeriodicSaveStateComponent(TimeSpan.FromMinutes(1)));
        }

        public async Task<BlockState> GetBlockState(int x, int y, int z)
        {
            await EnsureAroundChunkPopulated();
            return State.Storage[x, y, z];
        }

        public async Task<BlockState> GetBlockStateUnsafe(int x, int y, int z)
        {
            await EnsureChunkGenerated();
            return State.Storage[x, y, z];
        }

        public async Task<List<BlockState>> GetBlockStateListUnsafe(List<BlockChunkPos> blockPos)
        {
            await EnsureChunkGenerated();
            List<BlockState> ret = new List<BlockState>();
            foreach (var eachPos in blockPos)
            {
                ret.Add(State.Storage[eachPos.X, eachPos.Y, eachPos.Z]);
            }

            return ret;
        }

        public async Task ApplyChangeUnsafe(List<BlockStateChange> blockChanges)
        {
            await EnsureChunkGenerated();

            foreach (var eachChange in blockChanges)
            {
                var chunkPos = eachChange.Position.ToChunkWorldPos();

                // filter pos not in the chunk
                if (chunkPos != this.GetChunkWorldPos())
                    continue;
                var blockChunkPos = eachChange.Position.ToBlockChunkPos();
                if (eachChange.Condition.Contains(State.Storage[blockChunkPos.X, blockChunkPos.Y, blockChunkPos.Z]))
                {
                    State.Storage[blockChunkPos.X, blockChunkPos.Y, blockChunkPos.Z] = eachChange.State;
                }
            }
        }

        // Why is this the "Safe" version, and the other "Unsafe"?
        // Safe checks nearby chunks while Unsafe checks only this chunk, but even if it only checked this chunk it should be safe???
        public async Task<ChunkColumnCompactStorage> GetState()
        {
            await EnsureAroundChunkPopulated();
            return State.Storage;
        }

        public async Task<ChunkColumnCompactStorage> GetStateUnsafe()
        {
            await EnsureChunkGenerated();
            return State.Storage;
        }

        public async Task<BiomeId> GetBlockBiome(int x, int y, int z)
        {
            await EnsureChunkGenerated();
            return (BiomeId)State.Storage.Biomes[((y / 4) * 4 + z / 4) * 4 + x / 4];
        }

        public static readonly (int X, int Z)[] CrossCoords = new[]
        {
            (-1, 0), (0, -1), (1, 0), (0, 1)
        };

        public async Task SetBlockState(int x, int y, int z, BlockState blockState)
        {
            await EnsureAroundChunkPopulated();
            var state = State;
            var oldState = state.Storage[x, y, z];

            if (oldState != blockState)
            {
                state.Storage[x, y, z] = blockState;

                var chunkPos = new BlockChunkPos(x, y, z);
                var blockWorldPos = chunkPos.ToBlockWorldPos(ChunkWorldPos);
                await GetBroadcastGenerator().BlockChange(blockWorldPos, blockState);

                if (oldState.Id != blockState.Id)
                {
                    bool replaceOld = true;
                    var newEntity = BlockEntity.Create(GrainFactory, (BlockId)blockState.Id);

                    // 删除旧的 BlockEntity
                    if (state.BlockEntities.TryGetValue(chunkPos, out var entity))
                    {
                        if (object.Equals(entity, newEntity))
                            replaceOld = false;

                        if (replaceOld)
                        {
                            await entity.Tell(DestroyBlockEntity.Default);
                            state.BlockEntities.Remove(chunkPos);
                        }
                    }

                    // 添加新的 BlockEntity
                    if (newEntity != null && replaceOld)
                    {
                        state.BlockEntities.Add(chunkPos, newEntity);
                        await newEntity.Tell(new SpawnBlockEntity { World = World, Position = blockWorldPos });
                    }
                }

                // 通知周围 Block 更改
                await Task.WhenAll(CrossCoords.Select(crossCoord =>
                {
                    var neighborPos = blockWorldPos;
                    neighborPos.X += crossCoord.X;
                    neighborPos.Z += crossCoord.Z;
                    var chunk = neighborPos.ToChunkWorldPos();
                    var blockChunkPos = neighborPos.ToBlockChunkPos();
                    return GrainFactory.GetPartitionGrain<IChunkColumn>(World, chunk).OnBlockNeighborChanged(
                        blockChunkPos.X, blockChunkPos.Y, blockChunkPos.Z, blockWorldPos, oldState, blockState);
                }));
                MarkDirty();
            }
        }

        public async Task SetBlockStateUnsafe(int x, int y, int z, BlockState blockState)
        {
            await EnsureChunkGenerated();
            var oldState = State.Storage[x, y, z];

            if (oldState != blockState)
            {
                State.Storage[x, y, z] = blockState;
                MarkDirty();
            }
        }

        public async Task SetBlockStateListUnsafe(List<BlockChunkPos> blockPos, BlockState blockState)
        {
            await EnsureChunkGenerated();

            foreach (var eachPos in blockPos)
            {
                var oldState = State.Storage[eachPos.X, eachPos.Y, eachPos.Z];

                if (oldState != blockState)
                {
                    State.Storage[eachPos.X, eachPos.Y, eachPos.Z] = blockState;
                }
            }

            MarkDirty();
        }

        public Task EnsureChunkGenerated()
        {
            // Fast path: If already generated, return a completed task immediately.
            if (State.Generated)
            {
                return Task.CompletedTask;
            }

            // If generation is not complete, check if it's already in progress.
            // This is the key part of the async lazy initialization pattern.
            if (_generationTask != null)
            {
                // Another call is already generating, just await its completion.
                return _generationTask;
            }

            // We are the first, so start the generation task.
            return _generationTask = GenerateChunk();
        }

        private async Task GenerateChunk()
        {
            try
            {
                // This check is now inside the locked-down execution path, but
                // it's good practice for clarity and to handle potential edge cases.
                if (State.Generated) return;

                var serverSetting = GrainFactory.GetGrain<IServerSettings>(0);
                string worldType = (await serverSetting.GetSettings()).LevelType;
                if (worldType == "DEFAULT" || worldType == "default")
                {
                    var generator = GrainFactory.GetGrain<IChunkGeneratorOverworld>(await World.GetSeed());
                    GeneratorSettings settings = new GeneratorSettings { };
                    State.Storage = await generator.Generate(World, ChunkWorldPos.X, ChunkWorldPos.Z, settings);
                }
                else if (worldType == "FLAT" || worldType == "flat")
                {
                    var generator = GrainFactory.GetGrain<IChunkGeneratorFlat>(await World.GetSeed());
                    GeneratorSettings settings = new GeneratorSettings
                    {
                        FlatBlockId = new BlockState?[]
                        {
                            BlockStates.Bedrock(), BlockStates.Stone(), BlockStates.Stone(),
                            BlockStates.Dirt(), BlockStates.Dirt(), BlockStates.GrassBlock()
                        }
                    };
                    State.Storage = await generator.Generate(World, ChunkWorldPos.X, ChunkWorldPos.Z, settings);
                }
                else
                {
                    var generator = GrainFactory.GetGrain<IChunkGeneratorOverworld>(await World.GetSeed());
                    GeneratorSettings settings = new GeneratorSettings { };
                    State.Storage = await generator.Generate(World, ChunkWorldPos.X, ChunkWorldPos.Z, settings);
                }

                for (int x = 0; x < 16; ++x)
                {
                    for (int z = 0; z < 16; ++z)
                    {
                        State.Storage.GroundHeight[x, z] = GroundHeight(x, z);
                    }
                }

                State.Generated = true;
                await WriteStateAsync();
            }
            finally
            {
                // Crucially, clear the task so if it failed, it can be retried.
                _generationTask = null;
            }
        }

        public async Task EnsureChunkPopulated()
        {
            // Must be generated before it can be populated.
            await EnsureChunkGenerated();

            // Fast path: If already populated, we're done.
            if (State.Populated)
            {
                return;
            }

            // Check if population is already in progress.
            if (_populationTask != null)
            {
                await _populationTask;
                return;
            }

            // Start the population task.
            _populationTask = PopulateChunk();
            await _populationTask;
        }

        private async Task PopulateChunk()
        {
            try
            {
                if (State.Populated) return;

                var serverSetting = GrainFactory.GetGrain<IServerSettings>(0);
                string worldType = (await serverSetting.GetSettings()).LevelType;
                if (worldType.Equals("default", StringComparison.InvariantCultureIgnoreCase))
                {
                    var generator = GrainFactory.GetGrain<IChunkGeneratorOverworld>(await World.GetSeed());
                    await generator.Populate(World, ChunkWorldPos.X, ChunkWorldPos.Z, new GeneratorSettings());
                }
                else if (worldType.Equals("flat", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Flat worlds typically have no population step, but we call it for consistency.
                    var generator = GrainFactory.GetGrain<IChunkGeneratorFlat>(await World.GetSeed());
                    await generator.Populate(World, ChunkWorldPos.X, ChunkWorldPos.Z, new GeneratorSettings
                    {
                        FlatBlockId = new BlockState?[]
                       {
                            BlockStates.Bedrock(), BlockStates.Stone(), BlockStates.Stone(),
                            BlockStates.Dirt(), BlockStates.Dirt(), BlockStates.GrassBlock()
                       }
                    });
                }
                else
                {
                    throw new System.NotSupportedException("Unknown world type in server setting file.");
                }

                State.Populated = true;
                await WriteStateAsync();
            }
            finally
            {
                // Clear the task to allow retries on failure.
                _populationTask = null;
            }
        }

        private async Task EnsureAroundChunkPopulated()
        {
            // Our own population logic is now safe.
            await EnsureChunkPopulated();

            (var worldKey, var chunkPos) = this.GetWorldAndChunkWorldPos();
            var world = GrainFactory.GetGrain<IWorld>(worldKey);

            // This part is now safe because the chunks being called have the same protection.
            var tasks = new List<Task>();
            int range = 1;
            for (int xOffset = -range; xOffset <= range; ++xOffset)
            {
                for (int zOffset = -range; zOffset <= range; ++zOffset)
                {
                    if (xOffset == 0 && zOffset == 0) continue;
                    var curChunkPos = new ChunkWorldPos(chunkPos.X + xOffset, chunkPos.Z + zOffset);
                    var chunkColumnKey = world.MakeAddressByPartitionKey(curChunkPos);
                    tasks.Add(GrainFactory.GetGrain<IChunkColumn>(chunkColumnKey).EnsureChunkPopulated());
                }
            }

            await Task.WhenAll(tasks);
        }

        protected ClientPlayPacketFactory GetBroadcastGenerator()
        {
            return new ClientPlayPacketFactory(GrainFactory.GetPartitionGrain<IChunkTrackingHub>(World, ChunkWorldPos), null);
        }

        public Task<IBlockEntity> GetBlockEntity(int x, int y, int z)
        {
            if (State.BlockEntities.TryGetValue(new BlockChunkPos(x, y, z), out var entity))
                return Task.FromResult(entity);
            return Task.FromResult<IBlockEntity>(null);
        }

        public Task<int> GetGroundHeight(int x, int z)
        {
            return Task.FromResult(State.Storage.GroundHeight[x, z]);
        }

        public Task OnBlockNeighborChanged(int x, int y, int z, BlockWorldPos neighborPosition, BlockState oldState, BlockState newState)
        {
            if (State.Generated)
            {
                var block = State.Storage[x, y, z];
                var blockHandler = BlockHandler.Create((BlockId)block.Id);
                var selfPosition = new BlockChunkPos(x, y, z).ToBlockWorldPos(ChunkWorldPos);
                return blockHandler.OnNeighborChanged(selfPosition, neighborPosition, oldState, newState, GrainFactory, World);
            }

            return Task.CompletedTask;
        }

        private int GroundHeight(int x, int z)
        {
            var storage = State.Storage;
            for (int y = 255; y >= 0; --y)
            {
                if (!storage[x, y, z].IsAir())
                {
                    return y + 1;
                }
            }

            return 0;
        }

        private void MarkDirty()
        {
            ValueStorage.IsDirty = true;
        }

        [Orleans.GenerateSerializer]
        internal class StateHolder
        {
            [Id(0)]
            public bool Populated { get; set; } = false;

            [Id(1)]
            public bool Generated { get; set; } = false;

            [Id(2)]
            public ChunkColumnCompactStorage Storage { get; set; }

            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
            [Id(3)]
            public Dictionary<BlockChunkPos, IBlockEntity> BlockEntities { get; set; }

            public StateHolder()
            {
            }

            public StateHolder(InitializeStateMark mark)
            {
                BlockEntities = new Dictionary<BlockChunkPos, IBlockEntity>();
            }
        }
    }
}
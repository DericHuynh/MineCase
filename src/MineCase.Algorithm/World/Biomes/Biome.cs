using System;
using System.Collections.Generic;
using MineCase.Algorithm.Noise;
using MineCase.Algorithm.World.Mine;
using MineCase.Algorithm.World.Plants;
using MineCase.Block;
using MineCase.Server.Game.Entities;
using MineCase.Server.World;
using MineCase.Server.World.EntitySpawner;
using MineCase.World;
using MineCase.World.Biomes;
using MineCase.World.Generation;
using MineCase.World.Plants;
using Orleans;

namespace MineCase.Algorithm.World.Biomes
{
    [Orleans.GenerateSerializer]
    public abstract class Biome
    {
        // Biome�йص�������������
        [Id(0)]
        protected GeneratorSettings _genSettings;
        [Id(1)]
        protected string _name;
        [Id(2)]
        protected BiomeId _biomeId;

        /** The base height of this biome. Default 0.1. */
        [Id(3)]
        protected float _baseHeight;

        /** The variation from the base height of the biome. Default 0.3. */
        [Id(4)]
        protected float _heightVariation;

        /** The temperature of this biome. */
        [Id(5)]
        protected float _temperature;

        /** The rainfall in this biome. */
        [Id(6)]
        protected float _rainfall;

        /** Color tint applied to water depending on biome */
        [Id(7)]
        protected int _waterColor;

        /** Set to true if snow is enabled for this biome. */
        [Id(8)]
        protected bool _enableSnow;

        /** Is true (default) if the biome support rain (desert and nether can't have rain) */
        [Id(9)]
        protected bool _enableRain;

        /** The block expected to be on the top of this biome */
        [Id(10)]
        public BlockState _topBlock = BlockStates.GrassBlock();

        /** The block to fill spots in when not on the top */
        [Id(11)]
        public BlockState _fillerBlock = BlockStates.Dirt();

        // ��������
        protected static readonly OctavedNoise<PerlinNoise> _temperatureNoise =
            new OctavedNoise<PerlinNoise>(new PerlinNoise(1234), 4, 0.5F);

        protected static readonly OctavedNoise<PerlinNoise> _grassColorNoise =
            new OctavedNoise<PerlinNoise>(new PerlinNoise(2345), 4, 0.5F);

        // ����������
        [Id(12)]
        private MinableGenerator _dirtGen; // ��û��������Щ������������
        [Id(13)]
        private MinableGenerator _gravelOreGen;
        [Id(14)]
        private MinableGenerator _graniteGen;
        [Id(15)]
        private MinableGenerator _dioriteGen;
        [Id(16)]
        private MinableGenerator _andesiteGen;
        [Id(17)]
        private MinableGenerator _coalGen;
        [Id(18)]
        private MinableGenerator _ironGen;
        [Id(19)]
        private MinableGenerator _goldGen;
        [Id(20)]
        private MinableGenerator _redstoneGen;
        [Id(21)]
        private MinableGenerator _diamondGen;
        [Id(22)]
        private MinableGenerator _lapisGen;

        // ֲ������
        [Id(23)]
        protected int _treesPerChunk;
        [Id(24)]
        protected float _extraTreeChance;
        [Id(25)]
        protected int _grassPerChunk;
        [Id(26)]
        protected int _flowersPerChunk;
        [Id(27)]
        protected int _mushroomsPerChunk;
        [Id(28)]
        protected int _deadBushPerChunk;
        [Id(29)]
        protected int _reedsPerChunk;
        [Id(30)]
        protected int _cactiPerChunk;

        // ��������
        [Id(31)]
        protected List<MobType> _passiveMobList;
        [Id(32)]
        protected List<MobType> _monsterList;
        [Id(33)]
        protected int _clayPerChunk;
        [Id(34)]
        protected int _waterlilyPerChunk;
        [Id(35)]
        protected int _sandPatchesPerChunk;
        [Id(36)]
        protected int _gravelPatchesPerChunk;

        public Biome(BiomeProperties properties, GeneratorSettings genSettings)
        {
            _genSettings = genSettings;

            _name = properties.BiomeName;
            _biomeId = properties.BiomeId;
            _baseHeight = properties.BaseHeight;
            _heightVariation = properties.HeightVariation;
            _temperature = properties.Temperature;
            _rainfall = properties.Rainfall;
            _waterColor = properties.WaterColor;
            _enableSnow = properties.EnableSnow;
            _enableRain = properties.EnableRain;

            _dirtGen = new MinableGenerator(
                BlockStates.Dirt(),
                genSettings.DirtSize);
            _gravelOreGen = new MinableGenerator(
                BlockStates.Gravel(),
                genSettings.GravelSize);
            _graniteGen = new MinableGenerator(
                BlockStates.Granite(),
                genSettings.GraniteSize);
            _dioriteGen = new MinableGenerator(
                BlockStates.Diorite(),
                genSettings.DioriteSize);
            _andesiteGen = new MinableGenerator(
                BlockStates.Andesite(),
                genSettings.AndesiteSize);
            _coalGen = new MinableGenerator(
                BlockStates.CoalOre(),
                genSettings.CoalSize);
            _ironGen = new MinableGenerator(
                BlockStates.IronOre(),
                genSettings.IronSize);
            _goldGen = new MinableGenerator(
                BlockStates.GoldOre(),
                genSettings.GoldSize);

            _redstoneGen = new MinableGenerator(
                BlockStates.RedstoneOre(),
                genSettings.RedstoneSize);
            _diamondGen = new MinableGenerator(
                BlockStates.DiamondOre(),
                genSettings.DiamondSize);
            _lapisGen = new MinableGenerator(
                BlockStates.LapisOre(),
                genSettings.LapisSize);

            _treesPerChunk = 0; // mc 0
            _extraTreeChance = 0.05F; // mc 0.05F
            _grassPerChunk = 10;
            _flowersPerChunk = 4;
            _mushroomsPerChunk = 0;

            _deadBushPerChunk = 2;
            _reedsPerChunk = 50;
            _cactiPerChunk = 10;

            _passiveMobList = new List<MobType>();
            _monsterList = new List<MobType>();
        }

        public BiomeId GetBiomeId()
        {
            return _biomeId;
        }

        public string GetBiomeName()
        {
            return _name;
        }

        public float GetBaseHeight()
        {
            return _baseHeight;
        }

        public float GetHeightVariation()
        {
            return _heightVariation;
        }

        public static Biome GetBiome(int id, GeneratorSettings settings)
        {
            BiomeId biomeId = (BiomeId)id;
            switch (biomeId)
            {
                case BiomeId.Ocean:
                     return new BiomeOcean(new BiomeProperties(), settings);
                case BiomeId.Plains:
                     return new BiomePlains(new BiomeProperties(), settings);
                case BiomeId.Desert:
                     return new BiomeDesert(new BiomeProperties(), settings);
                case BiomeId.Mountains:
                     return new BiomeHill(BiomeHillType.Normal, new BiomeProperties(), settings);
                case BiomeId.Forest:
                     return new BiomeForest(new BiomeProperties(), settings);
                case BiomeId.Taiga:
                     return new BiomeTaiga(BiomeTaigaType.Normal, new BiomeProperties(), settings);
                case BiomeId.Swamp:
                     return new BiomeSwamp(new BiomeProperties(), settings);
                case BiomeId.River:
                     return new BiomeRiver(new BiomeProperties(), settings);
                case BiomeId.Beach:
                     return new BiomeBeach(new BiomeProperties(), settings);
                case BiomeId.Savanna:
                     return new BiomeSavanna(new BiomeProperties(), settings);
                default:
                     return null;
            }
        }

        // ������һ��������Ⱥϵ���ܳ��ֵĲ�
        public virtual PlantsType GetRandomGrass(Random rand)
        {
            return PlantsType.TallGrass;
        }

        // ������һ��������Ⱥϵ���ܳ��ֵĻ�
        public virtual PlantsType GetRandomFlower(Random rand)
        {
            double n = rand.NextDouble();
            if (n > 0.5)
            {
                return PlantsType.Poppy;
            }
            else
            {
                return PlantsType.Dandelion;
            }
        }

        // ������һ��������Ⱥϵ���ܳ��ֵ���
        public virtual PlantsType GetRandomTree(Random rand)
        {
            int n = rand.Next(2);
            switch (n)
            {
                case 0:
                    return PlantsType.Oak;
                case 1:
                    return PlantsType.Birch;
                default:
                    return PlantsType.Oak;
            }
        }

        public void GenerateOre(MinableGenerator generator, IWorld world, IGrainFactory grainFactory, ChunkColumnCompactStorage chunk, Random random, BlockWorldPos position, int count, int minHeight, int maxHeight)
        {
            if (minHeight > maxHeight)
            {
                int tmp = minHeight;
                minHeight = maxHeight;
                maxHeight = tmp;
            }
            else if (maxHeight == minHeight)
            {
                if (minHeight < 255)
                    ++maxHeight;
                else
                    --minHeight;
            }

            for (int j = 0; j < count; ++j)
            {
                BlockWorldPos blockpos = BlockWorldPos.Add(
                    position,
                    random.Next(16),
                    random.Next(maxHeight - minHeight) + minHeight,
                    random.Next(16));
                generator.Generate(world, grainFactory, chunk, random, blockpos);
            }
        }

        // ��������һЩ���飬Biome������Ҫ���ɿ���
        public virtual void Decorate(IWorld world, IGrainFactory grainFactory, ChunkColumnCompactStorage chunk, Random rand, BlockWorldPos pos)
        {
            GenerateOre(_dirtGen, world, grainFactory, chunk, rand, pos, _genSettings.DirtCount, _genSettings.DirtMaxHeight, _genSettings.DirtMinHeight);
            GenerateOre(_gravelOreGen, world, grainFactory, chunk, rand, pos, _genSettings.GravelCount, _genSettings.GravelMaxHeight, _genSettings.GravelMinHeight);
            GenerateOre(_graniteGen, world, grainFactory, chunk, rand, pos, _genSettings.GraniteCount, _genSettings.GraniteMaxHeight, _genSettings.GraniteMinHeight);
            GenerateOre(_dioriteGen, world, grainFactory, chunk, rand, pos, _genSettings.DioriteCount, _genSettings.DioriteMaxHeight, _genSettings.DioriteMinHeight);
            GenerateOre(_andesiteGen, world, grainFactory, chunk, rand, pos, _genSettings.AndesiteCount, _genSettings.AndesiteMaxHeight, _genSettings.AndesiteMinHeight);

            GenerateOre(_coalGen, world, grainFactory, chunk, rand, pos, _genSettings.CoalCount, _genSettings.CoalMaxHeight, _genSettings.CoalMinHeight);
            GenerateOre(_ironGen, world, grainFactory, chunk, rand, pos, _genSettings.IronCount, _genSettings.IronMaxHeight, _genSettings.IronMinHeight);
            GenerateOre(_goldGen, world, grainFactory, chunk, rand, pos, _genSettings.GoldCount, _genSettings.GoldMaxHeight, _genSettings.GoldMinHeight);
            GenerateOre(_redstoneGen, world, grainFactory, chunk, rand, pos, _genSettings.RedstoneCount, _genSettings.RedstoneMaxHeight, _genSettings.RedstoneMinHeight);
            GenerateOre(_diamondGen, world, grainFactory, chunk, rand, pos, _genSettings.DiamondCount, _genSettings.DiamondMaxHeight, _genSettings.DiamondMinHeight);
            GenerateOre(_lapisGen, world, grainFactory, chunk, rand, pos, _genSettings.LapisCount, _genSettings.LapisCenterHeight + _genSettings.LapisSpread, _genSettings.LapisCenterHeight - _genSettings.LapisSpread);
        }

        // ��������Ⱥϵ���е�����
        public virtual void SpawnMob(IWorld world, IGrainFactory grainFactory, ChunkColumnCompactStorage chunk, Random rand, BlockWorldPos pos)
        {
            ChunkWorldPos chunkPos = pos.ToChunkWorldPos();
            int seed = chunkPos.Z * 16384 + chunkPos.X;
            Random r = new Random(seed);
            foreach (MobType eachType in _passiveMobList)
            {
                if (r.Next(64) == 0)
                {
                    PassiveMobSpawner spawner = new PassiveMobSpawner(eachType, 10);
                    spawner.Spawn(world, grainFactory, chunk, rand, new BlockWorldPos(pos.X, pos.Y, pos.Z));
                }
            }
        }

        // ��������Ⱥϵ���еĹ���
        public virtual void SpawnMonster(IWorld world, IGrainFactory grainFactory, IChunkColumnStorage chunk, Random rand, BlockWorldPos pos)
        {
            ChunkWorldPos chunkPos = pos.ToChunkWorldPos();
            int seed = chunkPos.Z * 16384 + chunkPos.X;
            Random r = new Random(seed);
            foreach (MobType eachType in _monsterList)
            {
                if (r.Next(64) == 0)
                {
                    MonsterSpawner spawner = new MonsterSpawner(eachType, 3);
                    spawner.Spawn(world, grainFactory, chunk, rand, new BlockWorldPos(pos.X, pos.Y, pos.Z));
                }
            }
        }

        // ��������Ⱥϵ���еķ���
        public virtual void GenerateBiomeTerrain(int seaLevel, Random rand, ChunkColumnStorage chunk, int chunk_x, int chunk_z, int x_in_chunk, int z_in_chunk, double noiseVal)
        {
            BlockState topBlockstate = _topBlock;
            BlockState fillerBlockstate = _fillerBlock;
            int surfaceFlag = -1;
            int surfaceDepth = (int)(noiseVal / 3.0D + 3.0D + rand.NextDouble() * 0.25D);

            for (int y = 255; y >= 0; --y)
            {
                if (y <= rand.Next(5))
                {
                    chunk[x_in_chunk, y, z_in_chunk] = BlockStates.Bedrock();
                }
                else
                {
                    BlockState iblockstate = chunk[x_in_chunk, y, z_in_chunk];

                    if (iblockstate.IsAir())
                    {
                        surfaceFlag = -1;
                    }
                    else if (iblockstate == BlockStates.Stone())
                    {
                        // ������ʯͷ��������Ⱥϵ�滻
                        if (surfaceFlag == -1)
                        {
                            if (surfaceDepth <= 0)
                            {
                                topBlockstate = BlockStates.Air();
                                fillerBlockstate = BlockStates.Stone();
                            }
                            else if (y >= seaLevel - 4 && y <= seaLevel + 1)
                            {
                                topBlockstate = _topBlock;
                                fillerBlockstate = _fillerBlock;
                            }

                            // TODO �����¶ȱ仯����ˮ��״̬
                            surfaceFlag = surfaceDepth;

                            if (y >= seaLevel - 1)
                            {
                                chunk[x_in_chunk, y, z_in_chunk] = topBlockstate;
                            }
                            else if (y < seaLevel - 7 - surfaceDepth)
                            {
                                topBlockstate = BlockStates.Air();
                                fillerBlockstate = BlockStates.Stone();
                                chunk[x_in_chunk, y, z_in_chunk] = BlockStates.Gravel();
                            }
                            else
                            {
                                chunk[x_in_chunk, y, z_in_chunk] = fillerBlockstate;
                            }
                        }
                        else if (surfaceFlag > 0)
                        {
                            --surfaceFlag;
                            chunk[x_in_chunk, y, z_in_chunk] = fillerBlockstate;
                        }
                    }
                }
            }
        }
    }
}
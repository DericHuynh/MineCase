using System;
using MineCase.Block;

namespace MineCase.World.Generation
{
    [Orleans.GenerateSerializer]
    public class GeneratorSettings
    {
        [Orleans.Id(0)]
        public bool GenerateStructure { get; set; } = true;

        [Orleans.Id(1)]
        public bool UseCaves { get; set; } = true;

        [Orleans.Id(2)]
        public bool UseRavines { get; set; } = true;

        [Orleans.Id(3)]
        public bool UseMineShafts { get; set; } = true;

        [Orleans.Id(4)]
        public bool UseVillages { get; set; } = true;

        [Orleans.Id(5)]
        public bool UseStrongholds { get; set; } = true;

        [Orleans.Id(6)]
        public bool UseTemples { get; set; } = true;

        [Orleans.Id(7)]
        public bool UseMonuments { get; set; } = true;

        [Orleans.Id(8)]
        public bool UseMansions { get; set; } = true;

        [Orleans.Id(9)]
        public int SeaLevel { get; set; } = 63;

        [Orleans.Id(10)]
        public float DepthNoiseScaleX { get; set; } = 200.0F;

        [Orleans.Id(11)]
        public float DepthNoiseScaleY { get; set; } = 200.0F;

        [Orleans.Id(12)]
        public float DepthNoiseScaleZ { get; set; } = 200.0F;

        [Orleans.Id(13)]
        public float CoordinateScale { get; set; } = 0.05F; // mc = 684.412F;0.05

        [Orleans.Id(14)]
        public float HeightScale { get; set; } = 0.05F; // mc = 684.412F;0.05

        [Orleans.Id(15)]
        public float MainNoiseScaleX { get; set; } = 80.0F;

        [Orleans.Id(16)]
        public float MainNoiseScaleY { get; set; } = 160.0F;

        [Orleans.Id(17)]
        public float MainNoiseScaleZ { get; set; } = 80.0F;

        [Orleans.Id(18)]
        public float BiomeDepthOffSet { get; set; }

        [Orleans.Id(19)]
        public float BiomeDepthWeight { get; set; } = 1.0F;

        [Orleans.Id(20)]
        public float BiomeScaleOffset { get; set; }

        [Orleans.Id(21)]
        public float BiomeScaleWeight { get; set; } = 1.0F;

        [Orleans.Id(22)]
        public float BaseSize { get; set; } = 8.5F;

        [Orleans.Id(23)]
        public float StretchY { get; set; } = 12.0F;

        [Orleans.Id(24)]
        public float LowerLimitScale { get; set; } = 512.0F;

        [Orleans.Id(25)]
        public float UpperLimitScale { get; set; } = 512.0F;

        [Orleans.Id(26)]
        public int BiomeSize { get; set; } = 4;

        [Orleans.Id(27)]
        public int RiverSize { get; set; } = 4;

        // ores like
        [Orleans.Id(28)]
        public int DirtSize { get; set; } = 33;

        [Orleans.Id(29)]
        public int DirtCount { get; set; } = 10;

        [Orleans.Id(30)]
        public int DirtMinHeight { get; set; } = 0;

        [Orleans.Id(31)]
        public int DirtMaxHeight { get; set; } = 256;

        [Orleans.Id(32)]
        public int GravelSize { get; set; } = 33;

        [Orleans.Id(33)]
        public int GravelCount { get; set; } = 8;

        [Orleans.Id(34)]
        public int GravelMinHeight { get; set; } = 0;

        [Orleans.Id(35)]
        public int GravelMaxHeight { get; set; } = 256;

        [Orleans.Id(36)]
        public int GraniteSize { get; set; } = 33;

        [Orleans.Id(37)]
        public int GraniteCount { get; set; } = 10;

        [Orleans.Id(38)]
        public int GraniteMinHeight { get; set; } = 0;

        [Orleans.Id(39)]
        public int GraniteMaxHeight { get; set; } = 80;

        [Orleans.Id(40)]
        public int DioriteSize { get; set; } = 33;

        [Orleans.Id(41)]
        public int DioriteCount { get; set; } = 10;

        [Orleans.Id(42)]
        public int DioriteMinHeight { get; set; } = 0;

        [Orleans.Id(43)]
        public int DioriteMaxHeight { get; set; } = 80;

        [Orleans.Id(44)]
        public int AndesiteSize { get; set; } = 33;

        [Orleans.Id(45)]
        public int AndesiteCount { get; set; } = 10;

        [Orleans.Id(46)]
        public int AndesiteMinHeight { get; set; } = 0;

        [Orleans.Id(47)]
        public int AndesiteMaxHeight { get; set; } = 80;

        // ores
        [Orleans.Id(48)]
        public int CoalSize { get; set; } = 17;

        [Orleans.Id(49)]
        public int CoalCount { get; set; } = 20;

        [Orleans.Id(50)]
        public int CoalMinHeight { get; set; } = 0;

        [Orleans.Id(51)]
        public int CoalMaxHeight { get; set; } = 128;

        [Orleans.Id(52)]
        public int IronSize { get; set; } = 9;

        [Orleans.Id(53)]
        public int IronCount { get; set; } = 20;

        [Orleans.Id(54)]
        public int IronMinHeight { get; set; } = 0;

        [Orleans.Id(55)]
        public int IronMaxHeight { get; set; } = 64;

        [Orleans.Id(56)]
        public int GoldSize { get; set; } = 9;

        [Orleans.Id(57)]
        public int GoldCount { get; set; } = 2;

        [Orleans.Id(58)]
        public int GoldMinHeight { get; set; } = 0;

        [Orleans.Id(59)]
        public int GoldMaxHeight { get; set; } = 32;

        [Orleans.Id(60)]
        public int RedstoneSize { get; set; } = 8;

        [Orleans.Id(61)]
        public int RedstoneCount { get; set; } = 8;

        [Orleans.Id(62)]
        public int RedstoneMinHeight { get; set; } = 0;

        [Orleans.Id(63)]
        public int RedstoneMaxHeight { get; set; } = 16;

        [Orleans.Id(64)]
        public int DiamondSize { get; set; } = 8;

        [Orleans.Id(65)]
        public int DiamondCount { get; set; } = 1;

        [Orleans.Id(66)]
        public int DiamondMinHeight { get; set; } = 0;

        [Orleans.Id(67)]
        public int DiamondMaxHeight { get; set; } = 16;

        [Orleans.Id(68)]
        public int LapisSize { get; set; } = 7;

        [Orleans.Id(69)]
        public int LapisCount { get; set; } = 1;

        [Orleans.Id(70)]
        public int LapisCenterHeight { get; set; } = 16;

        [Orleans.Id(71)]
        public int LapisSpread { get; set; } = 16;

        [Orleans.Id(72)]
        public BlockState?[] FlatBlockId { get; set; } =
            new BlockState?[] { BlockStates.Bedrock(), BlockStates.Stone(), BlockStates.Dirt(), BlockStates.GrassBlock() };

        // public FlatGeneratorInfo FlatGeneratorInfo { get; set; }

        // public OverworldGeneratorInfo OverworldGeneratorInfo { get; set; }
    }
}

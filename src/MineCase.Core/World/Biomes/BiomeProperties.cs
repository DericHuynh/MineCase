using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase.World.Biomes
{
    [Orleans.GenerateSerializer]
    public class BiomeProperties
    {
        [Orleans.Id(0)]
        public string BiomeName { get; set; } = "InvalidBiome";

        [Orleans.Id(1)]
        public BiomeId BiomeId { get; set; } = BiomeId.Ocean;

        /** The base height of this biome. Default 0.1. */
        [Orleans.Id(2)]
        public float BaseHeight { get; set; } = 0.1F;

        /** The variation from the base height of the biome. Default 0.2. */
        [Orleans.Id(3)]
        public float HeightVariation { get; set; } = 0.2F;

        /** The temperature of this biome. */
        [Orleans.Id(4)]
        public float Temperature { get; set; } = 0.5F;

        /** The rainfall in this biome. */
        [Orleans.Id(5)]
        public float Rainfall { get; set; } = 0.5F;

        /** Color tint applied to water depending on biome */
        [Orleans.Id(6)]
        public int WaterColor { get; set; } = 16777215;

        /** Set to true if snow is enabled for this biome. */
        [Orleans.Id(7)]
        public bool EnableSnow { get; set; } = false;

        /** Is true (default) if the biome support rain (desert and nether can't have rain) */
        [Orleans.Id(8)]
        public bool EnableRain { get; set; } = true;
    }
}

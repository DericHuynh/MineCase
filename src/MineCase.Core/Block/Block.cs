using System;
using System.Collections.Generic;
using System.Text;
using MineCase.Block.State;
using MineCase.Item;

namespace MineCase.Block
{
    [Orleans.GenerateSerializer]
    public abstract class Block
    {
        [Orleans.Id(0)]
        public Material.Material Material { get; set; }

        /** Is it a full block */
        [Orleans.Id(1)]
        public bool FullBlock { get; set; }

        /** How much light is subtracted for going through this block */
        [Orleans.Id(2)]
        public int LightOpacity { get; set; }

        [Orleans.Id(3)]
        public bool Translucent { get; set; }

        /** Amount of light emitted */
        [Orleans.Id(4)]
        public int LightValue { get; set; }

        /** Flag if block should use the brightest neighbor light value as its own */
        [Orleans.Id(5)]
        public bool UseNeighborBrightness { get; set; }

        /** Indicates how many hits it takes to break a block. */
        [Orleans.Id(6)]
        public float BlockHardness { get; set; }

        /** Indicates how much this block can resist explosions */
        [Orleans.Id(7)]
        public float BlockResistance { get; set; }

        [Orleans.Id(8)]
        public bool EnableStats { get; set; }

        /**
         * Flags whether or not this block is of a type that needs random ticking. Ref-counted by ExtendedBlockStorage in
         * order to broadly cull a chunk from the random chunk update list for efficiency's sake.
         */
        [Orleans.Id(9)]
        public bool NeedsRandomTick { get; set; }

        /** true if the Block contains a Tile Entity */
        [Orleans.Id(10)]
        public bool IsBlockContainer { get; set; }

        /** Sound of stepping on the block */
        [Orleans.Id(11)]
        public SoundType BlockSoundType { get; set; }

        [Orleans.Id(12)]
        public float BlockParticleGravity { get; set; }

        [Orleans.Id(13)]
        public String Name { get; set; }

        [Orleans.Id(14)]
        public BlockState BaseBlockState { get; set; }

        public static Block FromBlockState(BlockState blockState)
        {
            if (!Blocks.IntToBlock.ContainsKey((int)blockState.Id))
                return Blocks.Air;
            else
                return Blocks.IntToBlock[(int)blockState.Id];
        }
    }
}

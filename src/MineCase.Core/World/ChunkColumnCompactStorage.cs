using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MineCase.Block;
using MineCase.Nbt;
using MineCase.Nbt.Tags;
using MineCase.World.Biomes;

namespace MineCase.World
{
    public static class ChunkConstants
    {
        public const int ChunkHeight = 256;
        public const int SectionsPerChunk = 16;

        public const int BlockEdgeWidthInSection = 16;

        public const int BlocksInSection = BlockEdgeWidthInSection * BlockEdgeWidthInSection * BlockEdgeWidthInSection;

        public const int BlocksInChunk = BlocksInSection * SectionsPerChunk;
        public static readonly byte[] EmptyLights = Enumerable.Repeat(0, BlocksInSection / 2).Select(x => (byte)x).ToArray();
        public static readonly byte[] FullLights = Enumerable.Repeat(255, BlocksInSection / 2).Select(x => (byte)x).ToArray();
    }

    [Orleans.GenerateSerializer]
    public sealed class ChunkColumnCompactStorage : IChunkColumnStorage
    {
        [Orleans.Id(0)]
        public ChunkSectionCompactStorage[] Sections { get; } = new ChunkSectionCompactStorage[ChunkConstants.SectionsPerChunk];

        [Orleans.Id(1)]
        public int[,] GroundHeight { get; } = new int[ChunkConstants.BlockEdgeWidthInSection, ChunkConstants.BlockEdgeWidthInSection];

        [Orleans.Id(2)]
        public int[] Biomes { get; }

        public BlockState this[int x, int y, int z]
        {
            get => Sections[y / 16].Data[x, y % 16, z];
            set => Sections[y / 16].Data[x, y % 16, z] = value;
        }

        public int SectionBitMask
        {
            get
            {
                int mask = 0;
                int index = 0;
                while (index < ChunkConstants.SectionsPerChunk)
                {
                    mask <<= 1;
                    mask |= Sections[index++] != null ? 1 : 0;
                }

                return mask;
            }
        }

        public NbtCompound Heightmaps
        {
            get
            {
                long[] compactArray = new long[36];

                // Load WorldSurface to it
                for (int x = 0; x < ChunkConstants.BlockEdgeWidthInSection; ++x)
                {
                    for (int z = 0; z < ChunkConstants.BlockEdgeWidthInSection; ++z)
                    {
                        int i = z * ChunkConstants.BlockEdgeWidthInSection + x;

                        int maxEntryValue = (1 << 9) - 1;
                        int bitOffset = i * 9;
                        int ulongOfsset = bitOffset >> 6;
                        int ulongOfssetNext = (i + 1) * 9 - 1 >> 6;
                        int bitsLow = bitOffset ^ ulongOfsset << 6;
                        compactArray[ulongOfsset] = compactArray[ulongOfsset] & ~(maxEntryValue << bitsLow) | ((long)GroundHeight[x, z] & maxEntryValue) << bitsLow;
                        if (ulongOfsset != ulongOfssetNext)
                        {
                            int bitsHigh = 64 - bitsLow;
                            int entryOffset = 9 - bitsHigh;
                            compactArray[ulongOfssetNext] = compactArray[ulongOfssetNext] >> entryOffset << entryOffset | ((long)GroundHeight[x, z] & maxEntryValue) >> bitsHigh;
                        }
                    }
                }

                NbtCompound ret = new NbtCompound();
                ret.Add(new NbtLongArray(compactArray, "MOTION_BLOCKING"));
                return ret;
            }
        }

        public int SkyLightMask
        {
            get
            {
                int mask = 1 << 17;

                // The 18-bit mask: bit 0 is for section y=-1, bit 1 for y=0, ..., bit 16 for y=15.
                // Sections[i] corresponds to bit i + 1.
                for (int i = 0; i < ChunkConstants.SectionsPerChunk; i++)
                {
                    if (Sections[i] != null && Sections[i].SkyLight != null && !Sections[i].SkyLight.IsAllZero())
                    {
                        mask |= 1 << (i + 1);
                    }
                }

                return mask;
            }
        }

        public int EmptySkyLightMask => SkyLightMask ^ 0b11_1111_1111_1111_1111;

        public int BlockLightMask
        {
            get
            {
                int mask = 0;
                for (int i = 0; i < ChunkConstants.SectionsPerChunk; i++)
                {
                    // BlockLight is always initialized if the section exists.
                    if (Sections[i] != null && !Sections[i].BlockLight.IsAllZero())
                    {
                        mask |= 1 << (i + 1);
                    }
                }

                return mask;
            }
        }

        public int EmptyBlockLightMask => BlockLightMask ^ 0b11_1111_1111_1111_1111;

        public List<byte[]> GetSkyLightArrays()
        {
            int skyLightMask = SkyLightMask;
            List<byte> indices = new List<byte>();

            for (int i = 0; i < ChunkConstants.SectionsPerChunk; i++)
            {
                // Check the bit for section 'i'.
                if ((skyLightMask & (1 << (i + 1))) != 0)
                    indices.Add((byte)i);
            }

            List<byte[]> lightArrays = new List<byte[]>(indices.Count + 1);

            foreach (byte index in indices)
            {
                lightArrays.Add(Sections[index].SkyLight.Storage);
            }

            // Temporarily add the out of chunk skylight
            lightArrays.Add(ChunkConstants.FullLights);

            return lightArrays;
        }

        public List<byte[]> GetBlockLightArrays()
        {
            int blockLightMask = BlockLightMask;
            List<byte> indices = new List<byte>();

            for (int i = 0; i < ChunkConstants.SectionsPerChunk; i++)
            {
                // Check the bit for section 'i'.
                if ((blockLightMask & (1 << (i + 1))) != 0)
                    indices.Add((byte)i);
            }

            List<byte[]> lightArrays = new List<byte[]>(indices.Count + 1);

            foreach (byte index in indices)
            {
                lightArrays.Add(Sections[index].BlockLight.Storage);
            }

            return lightArrays;
        }

        public ChunkColumnCompactStorage()
        {
            Biomes = new int[1024];
            for (int i = 0; i < Biomes.Length; ++i)
            {
                Biomes[i] = (int)BiomeId.Plains;
            }
        }

        public ChunkColumnCompactStorage(int[] biomes)
        {
            Biomes = biomes;
        }
    }

    [Orleans.GenerateSerializer]
    public sealed class ChunkSectionCompactStorage
    {
        private const byte _bitsPerBlock = 14;
        public const ulong BlockMask = (1u << _bitsPerBlock) - 1;
        [Orleans.Id(0)]
        private short _nonAirBlockCount = 4096; // FIXME: count block non air

        public byte BitsPerBlock => _bitsPerBlock;

        [Orleans.Id(1)]
        public DataArray Data { get; }

        [Orleans.Id(2)]
        public NibbleArray BlockLight { get; }

        [Orleans.Id(3)]
        public NibbleArray SkyLight { get; }

        public short NonAirBlockCount { get => _nonAirBlockCount; }

        public ChunkSectionCompactStorage(bool hasSkylight)
        {
            Data = new DataArray();
            BlockLight = new NibbleArray();
            if (hasSkylight)
                SkyLight = new NibbleArray();
        }

        public ChunkSectionCompactStorage(DataArray data, NibbleArray blockLight, NibbleArray skyLight)
        {
            Data = data;
            BlockLight = blockLight;
            SkyLight = skyLight;
        }

        [Orleans.GenerateSerializer]
        public sealed class DataArray
        {
            [Orleans.Id(0)]
            public ulong[] Storage { get; }

            public BlockState this[int x, int y, int z]
            {
                get
                {
                    if (y < 0 || y > 255)
                        return BlockStates.Air();
                    var offset = GetOffset(x, y, z);
                    var toRead = Math.Min(_bitsPerBlock, 64 - offset.BitOffset);
                    var value = Storage[offset.IndexOffset] >> offset.BitOffset;
                    var rest = _bitsPerBlock - toRead;
                    if (rest > 0)
                        value |= (Storage[offset.IndexOffset + 1] & ((1u << rest) - 1)) << toRead;
                    var blockState = BlockType.ParseBlockStateId((uint)(value & BlockMask));
                    return new BlockState { Id = blockState.Id, MetaValue = blockState.Meta };
                }

                set
                {
                    if (y < 0 || y > 255)
                        throw new IndexOutOfRangeException("Axis y out of range");
                    var stgValue = (ulong)((value.Id + value.MetaValue) & BlockMask);
                    var offset = GetOffset(x, y, z);
                    var tmpValue = Storage[offset.IndexOffset];
                    var mask = BlockMask << offset.BitOffset;
                    var toWrite = Math.Min(_bitsPerBlock, 64 - offset.BitOffset);
                    Storage[offset.IndexOffset] = (tmpValue & ~mask) | (stgValue << offset.BitOffset);
                    var rest = _bitsPerBlock - toWrite;
                    if (rest > 0)
                    {
                        mask = (1u << rest) - 1;
                        tmpValue = Storage[offset.IndexOffset + 1];
                        stgValue >>= toWrite;
                        Storage[offset.IndexOffset + 1] = (tmpValue & ~mask) | (stgValue & mask);
                    }
                }
            }

            public DataArray(ulong[] storage)
            {
                Storage = storage;
            }

            public DataArray()
            {
                Storage = new ulong[ChunkConstants.BlocksInSection * _bitsPerBlock / 64];
            }

            private static (int IndexOffset, int BitOffset) GetOffset(int x, int y, int z)
            {
                var index = GetBlockSerialIndex(x, y, z) * _bitsPerBlock;
                return (index / 64, index % 64);
            }
        }

        [Orleans.GenerateSerializer]
        public sealed class NibbleArray
        {
            [Orleans.Id(0)]
            public byte[] Storage { get; }

            public byte this[int x, int y, int z]
            {
                get
                {
                    var offset = GetBlockSerialIndex(x, y, z);
                    var value = Storage[offset / 2];
                    return offset % 2 == 0 ? (byte)((value >> 4) & 0xF) : (byte)(value & 0xF);
                }

                set
                {
                    var offset = GetBlockSerialIndex(x, y, z);
                    var tmpValue = Storage[offset / 2];
                    if (offset % 2 == 0)
                        Storage[offset / 2] = (byte)((tmpValue & 0xF) | (value << 4));
                    else
                        Storage[offset / 2] = (byte)((tmpValue & 0xF0) | (value & 0xF));
                }
            }

            public bool IsAllZero()
            {
                if (Storage == null) return true;
                foreach (byte b in Storage)
                {
                    if (b != 0)
                    {
                        return false;
                    }
                }

                return true;
            }

            public NibbleArray(byte[] storage)
            {
                Storage = storage;
                if (Storage is not null && Storage.Length > 2048)
                    throw new ArgumentOutOfRangeException("Light array is too big.");
            }

            public NibbleArray()
            {
                Storage = new byte[2048];
            }
        }

        private static int GetBlockSerialIndex(int x, int y, int z)
        {
            return ((y * ChunkConstants.BlockEdgeWidthInSection) + z) * ChunkConstants.BlockEdgeWidthInSection + x;
        }

        public static uint ToUInt32(ref BlockState blockState)
        {
            return blockState.Id + blockState.MetaValue;
        }
    }
}
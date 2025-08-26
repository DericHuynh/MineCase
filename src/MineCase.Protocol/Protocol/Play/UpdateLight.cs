using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x25)]
    [Orleans.GenerateSerializer]
    public sealed partial class UpdateLight : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public int ChunkX;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public int ChunkZ;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(2)]
        public int SkyLightMask;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(3)]
        public int BlockLightMask;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(4)]
        public int EmptySkyLightMask;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(5)]
        public int EmptyBlockLightMask;

        [SerializeAs(DataType.Array)]
        [Orleans.Id(6)]
        public LightArray[] SkyLights;

        [SerializeAs(DataType.Array)]
        [Orleans.Id(7)]
        public LightArray[] BlockLights;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsVarInt(ChunkX, out _);
            bw.WriteAsVarInt(ChunkZ, out _);
            bw.WriteAsVarInt(SkyLightMask, out _);
            bw.WriteAsVarInt(BlockLightMask, out _);
            bw.WriteAsVarInt(EmptySkyLightMask, out _);
            bw.WriteAsVarInt(EmptyBlockLightMask, out _);
            if (SkyLights != null)
                bw.WriteAsArray(SkyLights);
            if (SkyLights != null)
                bw.WriteAsArray(BlockLights);
        }

        public void Deserialize(ref SpanReader br)
        {
            // To do this, you need to get the lengths from the bit-masks.
            throw new NotSupportedException();
        }
    }
}

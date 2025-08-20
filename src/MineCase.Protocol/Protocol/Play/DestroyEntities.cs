using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x38)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class DestroyEntities : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint Count;

        [SerializeAs(DataType.VarIntArray, ArrayLengthMember = nameof(Count))]
        [Orleans.Id(1)]
        public uint[] EntityIds;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsVarInt(Count, out _);
            bw.WriteAsVarIntArray(EntityIds);
        }

        public void Deserialize(ref SpanReader br)
        {
            Count = br.ReadAsVarInt(out _);
            EntityIds = br.ReadAsVarIntArray((int)Count);
        }
    }
}

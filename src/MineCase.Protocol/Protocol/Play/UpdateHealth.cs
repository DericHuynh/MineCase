using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x49)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class UpdateHealth : IPacket
    {
        [SerializeAs(DataType.Float)]
        [Orleans.Id(0)]
        public float Health;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public uint Food;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(2)]
        public float FoodSaturation;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsFloat(Health);
            bw.WriteAsVarInt(Food, out _);
            bw.WriteAsFloat(FoodSaturation);
        }

        public void Deserialize(ref SpanReader br)
        {
            Health = br.ReadAsFloat();
            Food = br.ReadAsVarInt(out _);
            FoodSaturation = br.ReadAsFloat();
        }
    }
}

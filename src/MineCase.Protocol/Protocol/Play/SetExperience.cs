using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x48)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class SetExperience : IPacket
    {
        [SerializeAs(DataType.Float)]
        [Orleans.Id(0)]
        public float ExperienceBar;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public uint Level;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(2)]
        public uint TotalExperience;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsFloat(ExperienceBar);
            bw.WriteAsVarInt(Level, out _);
            bw.WriteAsVarInt(TotalExperience, out _);
        }

        public void Deserialize(ref SpanReader br)
        {
            ExperienceBar = br.ReadAsFloat();
            Level = br.ReadAsVarInt(out _);
            TotalExperience = br.ReadAsVarInt(out _);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x48)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class SetExperience : IPacket
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
    }
}

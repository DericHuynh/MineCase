using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x4F)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class TimeUpdate : IPacket
    {
        [SerializeAs(DataType.Long)]
        [Orleans.Id(0)]
        public long WorldAge;

        [SerializeAs(DataType.Long)]
        [Orleans.Id(1)]
        public long TimeOfDay;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsLong(WorldAge);
            bw.WriteAsLong(TimeOfDay);
        }

        public void Deserialize(ref SpanReader br)
        {
            WorldAge = br.ReadAsLong();
            TimeOfDay = br.ReadAsLong();
        }
    }
}

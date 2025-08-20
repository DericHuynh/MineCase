using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Status
{
    [Packet(0x01)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class Ping : IPacket
    {
        [SerializeAs(DataType.Long)]
        [Orleans.Id(0)]
        public long Payload;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsLong(Payload);
        }

        public void Deserialize(ref SpanReader br)
        {
            Payload = br.ReadAsLong();
        }
    }

    [Packet(0x01)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class Pong : IPacket
    {
        [SerializeAs(DataType.Long)]
        [Orleans.Id(0)]
        public long Payload;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsLong(Payload);
        }

        public void Deserialize(ref SpanReader br)
        {
            Payload = br.ReadAsLong();
        }
    }
}

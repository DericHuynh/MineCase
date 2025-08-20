using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x0F)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class ServerboundKeepAlive : IPacket
    {
        [SerializeAs(DataType.Long)]
        [Orleans.Id(0)]
        public long KeepAliveId;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsLong(KeepAliveId);
        }

        public void Deserialize(ref SpanReader br)
        {
            KeepAliveId = br.ReadAsLong();
        }
    }

    [Packet(0x21)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class ClientboundKeepAlive : IPacket
    {
        [SerializeAs(DataType.Long)]
        [Orleans.Id(0)]
        public long KeepAliveId;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsLong(KeepAliveId);
        }

        public void Deserialize(ref SpanReader br)
        {
            KeepAliveId = br.ReadAsLong();
        }
    }
}

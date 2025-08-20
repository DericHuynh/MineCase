using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x0A)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class ServerboundPluginMessage : IPacket
    {
        [SerializeAs(DataType.String)]
        [Orleans.Id(0)]
        public string Channel;

        [SerializeAs(DataType.ByteArray)]
        [Orleans.Id(1)]
        public byte[] Data;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsString(Channel);
            bw.WriteAsByteArray(Data);
        }

        public void Deserialize(ref SpanReader br)
        {
            Channel = br.ReadAsString();
            Data = br.ReadAsByteArray();
        }
    }
}

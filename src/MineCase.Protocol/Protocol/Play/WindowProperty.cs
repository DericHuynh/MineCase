using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x15)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class WindowProperty : IPacket
    {
        [SerializeAs(DataType.Byte)]
        [Orleans.Id(0)]
        public byte WindowId;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(1)]
        public short Property;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(2)]
        public short Value;

        public void Serialize(BinaryWriter bw)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(ref SpanReader br)
        {
            WindowId = br.ReadAsByte();
            Property = br.ReadAsShort();
            Value = br.ReadAsShort();
        }
    }
}

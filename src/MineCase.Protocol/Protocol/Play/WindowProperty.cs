using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x15)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class WindowProperty : IPacket
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
    }
}

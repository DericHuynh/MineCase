using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x0A)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class ServerboundPluginMessage : IPacket
    {
        [SerializeAs(DataType.String)]
        [Orleans.Id(0)]
        public string Channel;

        [SerializeAs(DataType.ByteArray)]
        [Orleans.Id(1)]
        public byte[] Data;
    }
}

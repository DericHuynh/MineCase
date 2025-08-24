using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Status
{
    [Packet(0x01)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class Ping : IPacket
    {
        [SerializeAs(DataType.Long)]
        [Orleans.Id(0)]
        public long Payload;
    }

    [Packet(0x01)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class Pong : IPacket
    {
        [SerializeAs(DataType.Long)]
        [Orleans.Id(0)]
        public long Payload;
    }
}

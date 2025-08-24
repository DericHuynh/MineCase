using System;
using System.Collections.Generic;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x2D)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class UseItem : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public Hand Hand;
    }
}

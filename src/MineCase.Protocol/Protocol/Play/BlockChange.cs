using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x0C)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class BlockChange : IPacket
    {
        [SerializeAs(DataType.Position)]
        [Orleans.Id(0)]
        public Position Location;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public uint BlockId;
    }
}

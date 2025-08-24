using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x00)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class TeleportConfirm : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint TeleportId;
    }
}

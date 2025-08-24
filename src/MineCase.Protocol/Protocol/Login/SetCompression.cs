using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Login
{
    [Packet(Protocol.SetCompressionPacketId)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class SetCompression : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint Threshold;
    }
}

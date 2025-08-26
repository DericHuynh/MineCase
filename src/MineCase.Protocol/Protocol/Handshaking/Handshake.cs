using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Handshaking
{
    [Packet(0x00)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class Handshake : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public int ProtocolVersion;

        [SerializeAs(DataType.String)]
        [Orleans.Id(1)]
        public string ServerAddress;

        [SerializeAs(DataType.UnsignedShort)]
        [Orleans.Id(2)]
        public ushort ServerPort;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(3)]
        public int NextState;
    }
}

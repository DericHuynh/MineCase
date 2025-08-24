using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x05)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class ClientSettings : IPacket
    {
        [SerializeAs(DataType.String)]
        [Orleans.Id(0)]
        public string Locale;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(1)]
        public byte ViewDistance;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(2)]
        public uint ChatMode;

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(3)]
        public bool ChatColors;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(4)]
        public byte DisplayedSkinParts;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(5)]
        public uint MainHand;
    }
}

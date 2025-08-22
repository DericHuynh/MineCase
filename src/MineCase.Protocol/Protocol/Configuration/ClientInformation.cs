using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Configuration
{
    [Packet(0x00)]
    [GenerateSerializer]
    public sealed partial class ServerBoundClientInformation : IPacket
    {
        [SerializeAs(DataType.String)]
        public string Locale;

        [SerializeAs(DataType.Byte)]
        public byte ViewDistance;

        [SerializeAs(DataType.Boolean)]
        public bool ChatMode;

        [SerializeAs(DataType.Boolean)]
        public bool ChatColors;

        [SerializeAs(DataType.Byte)]
        public byte DisplayedSkinParts;

        [SerializeAs(DataType.Boolean)]
        public bool MainHand;
    }
}

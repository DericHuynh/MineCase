using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x03)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class ServerboundChatMessage : IPacket
    {
        [SerializeAs(DataType.String)]
        [Orleans.Id(0)]
        public string Message;
    }

    // TODO
    [Packet(0x0F)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class ClientboundChatMessage : IPacket
    {
        [SerializeAs(DataType.Chat)]
        [Orleans.Id(0)]
        public Chat JSONData;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(1)]
        public byte Position; // 0: chat (chat box), 1: system message (chat box), 2: game info (above hotbar).
    }
}

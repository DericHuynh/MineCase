using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x2F)]
    public sealed class OpenWindow : IPacket
    {
        [SerializeAs(DataType.Byte)]
        public byte WindowId;

        [SerializeAs(DataType.String)]
        public string WindowType;

        [SerializeAs(DataType.Chat)]
        public Chat WindowTitle;

        [SerializeAs(DataType.Byte)]
        public byte NumberOfSlots;

        [SerializeAs(DataType.Byte)]
        public byte? EntityId;

        public void Deserialize(ref SpanReader br)
        {
            WindowId = br.ReadAsByte();
            WindowType = br.ReadAsString();
            WindowTitle = br.ReadAsChat();
            NumberOfSlots = br.ReadAsByte();
            if (!br.IsEmpty)
                EntityId = br.ReadAsByte();
            else
                EntityId = null;
        }

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsByte(WindowId);
            bw.WriteAsString(WindowType);
            bw.WriteAsChat(WindowTitle);
            bw.WriteAsByte(NumberOfSlots);
            if (EntityId.HasValue)
                bw.WriteAsByte(EntityId.Value);
        }
    }
}

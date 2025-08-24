using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x2F)]
    [Orleans.GenerateSerializer]
    public sealed class OpenWindow : IPacket
    {
        [SerializeAs(DataType.Byte)]
        [Orleans.Id(0)]
        public byte WindowId;

        [SerializeAs(DataType.String)]
        [Orleans.Id(1)]
        public string WindowType;

        [SerializeAs(DataType.Chat)]
        [Orleans.Id(2)]
        public Chat WindowTitle;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(3)]
        public byte NumberOfSlots;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(4)]
        public byte? EntityId;

        public void Deserialize(ref SpanReader br)
        {
            throw new NotImplementedException();
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

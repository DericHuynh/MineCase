using System;
using System.Collections.Generic;
using System.Text;

using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x09)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class ClickWindow : IPacket
    {
        [SerializeAs(DataType.Byte)]
        [Orleans.Id(0)]
        public byte WindowId;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(1)]
        public short Slot;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(2)]
        public byte Button;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(3)]
        public short ActionNumber;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(4)]
        public uint Mode;

        [SerializeAs(DataType.Slot)]
        [Orleans.Id(5)]
        public Slot ClickedItem;
    }
}

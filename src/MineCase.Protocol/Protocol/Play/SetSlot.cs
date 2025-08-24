using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x17)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class SetSlot : IPacket
    {
        [SerializeAs(DataType.Byte)]
        [Orleans.Id(0)]
        public byte WindowId;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(1)]
        public short Slot;

        [SerializeAs(DataType.Slot)]
        [Orleans.Id(2)]
        public Slot SlotData;
    }
}

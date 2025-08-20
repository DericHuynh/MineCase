using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x23)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class ServerboundHeldItemChange : IPacket
    {
        [SerializeAs(DataType.Short)]
        [Orleans.Id(0)]
        public short Slot;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsShort(Slot);
        }

        public void Deserialize(ref SpanReader br)
        {
            Slot = br.ReadAsShort();
        }
    }
}

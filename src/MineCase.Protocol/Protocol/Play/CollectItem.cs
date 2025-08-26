using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x56)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class CollectItem : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public int CollectedEntityId;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public int CollectorEntityId;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(2)]
        public int PickupItemCount;
    }
}

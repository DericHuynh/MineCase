using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x49)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class UpdateHealth : IPacket
    {
        [SerializeAs(DataType.Float)]
        [Orleans.Id(0)]
        public float Health;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public int Food;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(2)]
        public float FoodSaturation;
    }
}

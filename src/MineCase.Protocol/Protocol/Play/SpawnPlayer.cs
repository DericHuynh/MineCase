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
    public sealed partial class SpawnPlayer : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint EntityId;

        [SerializeAs(DataType.UUID)]
        [Orleans.Id(1)]
        public Guid PlayerUUID;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(2)]
        public double X;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(3)]
        public double Y;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(4)]
        public double Z;

        [SerializeAs(DataType.Angle)]
        [Orleans.Id(5)]
        public Angle Yaw;

        [SerializeAs(DataType.Angle)]
        [Orleans.Id(6)]
        public Angle Pitch;
    }
}

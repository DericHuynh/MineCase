using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x57)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class EntityTeleport : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public int EID;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(1)]
        public double X;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(2)]
        public double Y;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(3)]
        public double Z;

        [SerializeAs(DataType.Angle)]
        [Orleans.Id(4)]
        public Angle Yaw;

        [SerializeAs(DataType.Angle)]
        [Orleans.Id(5)]
        public Angle Pitch;

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(6)]
        public bool OnGround;
    }
}

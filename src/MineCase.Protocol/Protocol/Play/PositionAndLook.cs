using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x36)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class ClientboundPositionAndLook : IPacket
    {
        [SerializeAs(DataType.Double)]
        [Orleans.Id(0)]
        public double X;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(1)]
        public double Y;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(2)]
        public double Z;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(3)]
        public float Yaw;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(4)]
        public float Pitch;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(5)]
        public byte Flags;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(6)]
        public uint TeleportId;
    }

    [Packet(0x12)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class ServerboundPositionAndLook : IPacket
    {
        [SerializeAs(DataType.Double)]
        [Orleans.Id(0)]
        public double X;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(1)]
        public double FeetY;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(2)]
        public double Z;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(3)]
        public float Yaw;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(4)]
        public float Pitch;

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(5)]
        public bool OnGround;
    }
}

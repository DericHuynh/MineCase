using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    // FIXME : 1.15.2 does not have this packet
    [Packet(0x27)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class EntityLookAndRelativeMove : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint EID;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(1)]
        public short DeltaX;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(2)]
        public short DeltaY;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(3)]
        public short DeltaZ;

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

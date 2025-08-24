using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    // FIXME : 1.15.2 does not have this packet
    [Packet(0x28)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class EntityLook : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint EID;

        [SerializeAs(DataType.Angle)]
        [Orleans.Id(1)]
        public Angle Yaw;

        [SerializeAs(DataType.Angle)]
        [Orleans.Id(2)]
        public Angle Pitch;

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(3)]
        public bool OnGround;
    }
}

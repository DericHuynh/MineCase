using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    // FIXME : 1.15.2 does not have this packet
    [Packet(0x10)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class PlayerLook : IPacket
    {
        [SerializeAs(DataType.Float)]
        [Orleans.Id(0)]
        public float Yaw;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(1)]
        public float Pitch;

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(2)]
        public bool OnGround;
    }
}

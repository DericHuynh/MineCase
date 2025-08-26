using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x3C)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class EntityHeadLook : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public int EID;

        [SerializeAs(DataType.Angle)]
        [Orleans.Id(1)]
        public Angle Yaw;
    }
}

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
    [MineCase.Serialization.GenerateSerializer]
    public sealed class PlayerLook : IPacket
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

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsFloat(Yaw);
            bw.WriteAsFloat(Pitch);
            bw.WriteAsBoolean(OnGround);
        }

        public void Deserialize(ref SpanReader br)
        {
            Yaw = br.ReadAsFloat();
            Pitch = br.ReadAsFloat();
            OnGround = br.ReadAsBoolean();
        }
    }
}

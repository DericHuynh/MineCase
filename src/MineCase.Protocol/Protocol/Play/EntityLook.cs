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
    [MineCase.Serialization.GenerateSerializer]
    public sealed class EntityLook : IPacket
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

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsVarInt(EID, out _);
            bw.WriteAsAngle(Yaw);
            bw.WriteAsAngle(Pitch);
            bw.WriteAsBoolean(OnGround);
        }

        public void Deserialize(ref SpanReader br)
        {
            EID = br.ReadAsVarInt(out _);
            Yaw = br.ReadAsAngle();
            Pitch = br.ReadAsAngle();
            OnGround = br.ReadAsBoolean();
        }
    }
}

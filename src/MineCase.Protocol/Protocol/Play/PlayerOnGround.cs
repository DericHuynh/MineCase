using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    // FIXME : 1.15.2 does not have this packet
    [Packet(0x0D)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class PlayerOnGround : IPacket
    {
        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(0)]
        public bool OnGround;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsBoolean(OnGround);
        }

        public void Deserialize(ref SpanReader br)
        {
            OnGround = br.ReadAsBoolean();
        }
    }
}

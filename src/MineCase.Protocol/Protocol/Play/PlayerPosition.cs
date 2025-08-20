using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x11)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class PlayerPosition : IPacket
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

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(3)]
        public bool OnGround;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsDouble(X);
            bw.WriteAsDouble(FeetY);
            bw.WriteAsDouble(Z);
            bw.WriteAsBoolean(OnGround);
        }

        public void Deserialize(ref SpanReader br)
        {
            X = br.ReadAsDouble();
            FeetY = br.ReadAsDouble();
            Z = br.ReadAsDouble();
            OnGround = br.ReadAsBoolean();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x0A)]
    [Orleans.GenerateSerializer]
    public sealed class UseEntity : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public int Target;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public int Type;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(2)]
        public float? TargetX;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(3)]
        public float? TargetY;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(4)]
        public float? TargetZ;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(5)]
        public int? Hand;

        public void Deserialize(ref SpanReader br)
        {
            Target = (int)br.ReadAsVarInt(out _);
            Type = (int)br.ReadAsVarInt(out _);

            // Only if Type is interact at
            if (Type == 2)
            {
                TargetX = br.ReadAsFloat();
                TargetY = br.ReadAsFloat();
                TargetZ = br.ReadAsFloat();
            }

            // Only if Type is interact or interact at
            if (Type == 0 || Type == 2)
                Hand = (int)br.ReadAsVarInt(out _);
        }

        public void Serialize(BinaryWriter bw)
        {
            throw new NotImplementedException();
        }
    }
}

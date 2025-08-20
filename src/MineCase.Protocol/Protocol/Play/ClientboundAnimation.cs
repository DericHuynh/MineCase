using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x06)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class ClientboundAnimation : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint EntityID;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(1)]
        public ClientboundAnimationId AnimationID;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsVarInt(EntityID, out _);
            bw.WriteAsByte((byte)AnimationID);
        }

        public void Deserialize(ref SpanReader br)
        {
            EntityID = br.ReadAsVarInt(out _);
            AnimationID = (ClientboundAnimationId)br.ReadAsByte();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    public enum Hand : uint
    {
        Main = 0,
        Off = 1
    }

    [Packet(0x2A)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class ServerboundAnimation : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public Hand Hand;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsVarInt((uint)Hand, out _);
        }

        public void Deserialize(ref SpanReader br)
        {
            Hand = (Hand)br.ReadAsVarInt(out _);
        }
    }
}

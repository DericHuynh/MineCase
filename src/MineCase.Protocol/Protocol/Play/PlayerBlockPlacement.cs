using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x2C)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class PlayerBlockPlacement : IPacket
    {
        [SerializeAs(DataType.Position)]
        [Orleans.Id(0)]
        public Position Location;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public PlayerDiggingFace Face;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(2)]
        public Hand Hand;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(3)]
        public float CursorPositionX;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(4)]
        public float CursorPositionY;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(5)]
        public float CursorPositionZ;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsPosition(Location);
            bw.WriteAsVarInt((byte)Face, out _);
            bw.WriteAsVarInt((uint)Hand, out _);
            bw.WriteAsFloat(CursorPositionX);
            bw.WriteAsFloat(CursorPositionY);
            bw.WriteAsFloat(CursorPositionZ);
        }

        public void Deserialize(ref SpanReader br)
        {
            Location = br.ReadAsPosition();
            Face = (PlayerDiggingFace)br.ReadAsVarInt(out _);
            Hand = (Hand)br.ReadAsVarInt(out _);
            CursorPositionX = br.ReadAsFloat();
            CursorPositionY = br.ReadAsFloat();
            CursorPositionZ = br.ReadAsFloat();
        }
    }
}

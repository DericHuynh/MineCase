using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x05)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class ClientSettings : IPacket
    {
        [SerializeAs(DataType.String)]
        [Orleans.Id(0)]
        public string Locale;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(1)]
        public byte ViewDistance;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(2)]
        public uint ChatMode;

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(3)]
        public bool ChatColors;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(4)]
        public byte DisplayedSkinParts;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(5)]
        public uint MainHand;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsString(Locale);
            bw.WriteAsByte(ViewDistance);
            bw.WriteAsVarInt(ChatMode, out _);
            bw.WriteAsBoolean(ChatColors);
            bw.WriteAsByte(DisplayedSkinParts);
            bw.WriteAsVarInt(MainHand, out _);
        }

        public void Deserialize(ref SpanReader br)
        {
            Locale = br.ReadAsString();
            ViewDistance = br.ReadAsByte();
            ChatMode = br.ReadAsVarInt(out _);
            ChatColors = br.ReadAsBoolean();
            DisplayedSkinParts = br.ReadAsByte();
            MainHand = br.ReadAsVarInt(out _);
        }
    }
}

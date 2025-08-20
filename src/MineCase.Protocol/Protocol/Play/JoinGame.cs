using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x26)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class JoinGame : IPacket
    {
        [SerializeAs(DataType.Int)]
        [Orleans.Id(0)]
        public int EID;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(1)]
        public byte GameMode;

        [SerializeAs(DataType.Int)]
        [Orleans.Id(2)]
        public int Dimension;

        [SerializeAs(DataType.Long)]
        [Orleans.Id(3)]
        public long HashedSeed;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(4)]
        public byte MaxPlayers;

        [SerializeAs(DataType.String)]
        [Orleans.Id(5)]
        public string LevelType;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(6)]
        public uint ViewDistance;

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(7)]
        public bool ReducedDebugInfo;

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(8)]
        public bool EnableRespawnScreen;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsInt(EID);
            bw.WriteAsByte(GameMode);
            bw.WriteAsInt(Dimension);
            bw.WriteAsLong(HashedSeed);
            bw.WriteAsByte(MaxPlayers);
            bw.WriteAsString(LevelType);
            bw.WriteAsVarInt(ViewDistance, out _);
            bw.WriteAsBoolean(ReducedDebugInfo);
            bw.WriteAsBoolean(EnableRespawnScreen);
        }

        public void Deserialize(ref SpanReader br)
        {
            EID = br.ReadAsInt();
            GameMode = br.ReadAsByte();
            Dimension = br.ReadAsInt();
            HashedSeed = br.ReadAsLong();
            MaxPlayers = br.ReadAsByte();
            LevelType = br.ReadAsString();
            ViewDistance = br.ReadAsVarInt(out _);
            ReducedDebugInfo = br.ReadAsBoolean();
            EnableRespawnScreen = br.ReadAsBoolean();
        }
    }
}

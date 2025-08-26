using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x26)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class JoinGame : IPacket
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
        public int ViewDistance;

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(7)]
        public bool ReducedDebugInfo;

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(8)]
        public bool EnableRespawnScreen;
    }
}

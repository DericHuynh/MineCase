using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x3B)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class Respawn : IPacket
    {
        [SerializeAs(DataType.Int)]
        [Orleans.Id(0)]
        public int Dimension;

        [SerializeAs(DataType.Long)]
        [Orleans.Id(1)]
        public long HashedSeed;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(2)]
        public byte Gamemode;

        [SerializeAs(DataType.String)]
        [Orleans.Id(3)]
        public string LevelType;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsInt(Dimension);
            bw.WriteAsLong(HashedSeed);
            bw.WriteAsByte(Gamemode);
            bw.WriteAsString(LevelType);
        }

        public void Deserialize(ref SpanReader br)
        {
            Dimension = br.ReadAsInt();
            HashedSeed = br.ReadAsLong();
            Gamemode = br.ReadAsByte();
            LevelType = br.ReadAsString();
        }
    }
}

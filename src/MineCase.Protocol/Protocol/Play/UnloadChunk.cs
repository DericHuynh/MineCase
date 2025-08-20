using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x1E)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class UnloadChunk : IPacket
    {
        [SerializeAs(DataType.Int)]
        [Orleans.Id(0)]
        public int ChunkX;

        [SerializeAs(DataType.Int)]
        [Orleans.Id(1)]
        public int ChunkZ;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsInt(ChunkX);
            bw.WriteAsInt(ChunkZ);
        }

        public void Deserialize(ref SpanReader br)
        {
            ChunkX = br.ReadAsInt();
            ChunkZ = br.ReadAsInt();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x1E)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class UnloadChunk : IPacket
    {
        [SerializeAs(DataType.Int)]
        [Orleans.Id(0)]
        public int ChunkX;

        [SerializeAs(DataType.Int)]
        [Orleans.Id(1)]
        public int ChunkZ;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x41)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class UpdateViewPosition : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public int ChunkX;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public int ChunkZ;
    }
}

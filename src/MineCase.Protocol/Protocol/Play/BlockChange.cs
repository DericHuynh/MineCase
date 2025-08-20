using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x0C)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class BlockChange : IPacket
    {
        [SerializeAs(DataType.Position)]
        [Orleans.Id(0)]
        public Position Location;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public uint BlockId;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsPosition(Location);
            bw.WriteAsVarInt(BlockId, out _);
        }

        public void Deserialize(ref SpanReader br)
        {
            Location = br.ReadAsPosition();
            BlockId = br.ReadAsVarInt(out _);
        }
    }
}

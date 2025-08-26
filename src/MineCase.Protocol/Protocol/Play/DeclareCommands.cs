using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Command;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x12)]
    [Orleans.GenerateSerializer]
    public sealed class DeclareCommands : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public int Count;

        [SerializeAs(DataType.Array)]
        [Orleans.Id(1)]
        public Node[] Nodes;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(2)]
        public int RootIndex;

        // TODO : complete serialization and deserialization
        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsVarInt(Count, out _);
        }

        public void Deserialize(ref SpanReader br)
        {
            Count = br.ReadAsVarInt(out _);
        }
    }
}

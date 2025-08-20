using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x00)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class TeleportConfirm : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint TeleportId;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsVarInt(TeleportId, out _);
        }

        public void Deserialize(ref SpanReader br)
        {
            TeleportId = br.ReadAsVarInt(out _);
        }
    }
}

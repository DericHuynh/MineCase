using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x38)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class DestroyEntities : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint Count;

        [SerializeAs(DataType.VarIntArray, ArrayLengthMember = nameof(Count))]
        [Orleans.Id(1)]
        public uint[] EntityIds;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public partial class LightArray : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public int Length;

        [SerializeAs(DataType.ByteArray, ArrayLengthMember = nameof(Length))]
        [Orleans.Id(1)]
        public byte[] Lights;
    }
}

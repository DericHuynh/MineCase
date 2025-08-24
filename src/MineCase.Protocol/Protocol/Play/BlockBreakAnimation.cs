using System.IO;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x09)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class BlockBreakAnimation : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint EntityID;

        [SerializeAs(DataType.Position)]
        [Orleans.Id(1)]
        public Position BlockPosition;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(2)]
        public byte DestoryStage;
    }
}

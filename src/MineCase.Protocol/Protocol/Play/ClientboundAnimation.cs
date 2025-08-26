using System.IO;

using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x06)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class ClientboundAnimation : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public int EntityID;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(1)]
        public ClientboundAnimationId AnimationID;
    }
}

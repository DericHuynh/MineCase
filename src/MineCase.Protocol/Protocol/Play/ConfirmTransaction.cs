using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x06)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class ServerboundConfirmTransaction : IPacket
    {
        [SerializeAs(DataType.Byte)]
        [Orleans.Id(0)]
        public byte WindowId;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(1)]
        public short ActionNumber;

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(2)]
        public bool Accepted;
    }

    [Packet(0x11)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class ClientboundConfirmTransaction : IPacket
    {
        [SerializeAs(DataType.Byte)]
        [Orleans.Id(0)]
        public byte WindowId;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(1)]
        public short ActionNumber;

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(2)]
        public bool Accepted;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x06)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class ServerboundConfirmTransaction : IPacket
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

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsByte(WindowId);
            bw.WriteAsShort(ActionNumber);
            bw.WriteAsBoolean(Accepted);
        }

        public void Deserialize(ref SpanReader br)
        {
            WindowId = br.ReadAsByte();
            ActionNumber = br.ReadAsShort();
            Accepted = br.ReadAsBoolean();
        }
    }

    [Packet(0x11)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class ClientboundConfirmTransaction : IPacket
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

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsByte(WindowId);
            bw.WriteAsShort(ActionNumber);
            bw.WriteAsBoolean(Accepted);
        }

        public void Deserialize(ref SpanReader br)
        {
            WindowId = br.ReadAsByte();
            ActionNumber = br.ReadAsShort();
            Accepted = br.ReadAsBoolean();
        }
    }
}

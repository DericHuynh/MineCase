using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Login
{
    [Packet(0x00)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class LoginStart : IPacket
    {
        [SerializeAs(DataType.String)]
        [Orleans.Id(0)]
        public string Name;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsString(Name);
        }

        public void Deserialize(ref SpanReader br)
        {
            Name = br.ReadAsString();
        }
    }

    [Packet(0x00)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class LoginDisconnect : IPacket
    {
        [SerializeAs(DataType.String)]
        [Orleans.Id(0)]
        public string Reason;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsString(Reason);
        }

        public void Deserialize(ref SpanReader br)
        {
            Reason = br.ReadAsString();
        }
    }

    [Packet(0x02)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class LoginSuccess : IPacket
    {
        [SerializeAs(DataType.String)]
        [Orleans.Id(0)]
        public string UUID;

        [SerializeAs(DataType.String)]
        [Orleans.Id(1)]
        public string Username;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsString(UUID);
            bw.WriteAsString(Username);
        }

        public void Deserialize(ref SpanReader br)
        {
            UUID = br.ReadAsString();
            Username = br.ReadAsString();
        }
    }
}

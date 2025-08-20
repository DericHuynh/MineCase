using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Handshaking
{
    [Packet(0x00)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class Handshake : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint ProtocolVersion;

        [SerializeAs(DataType.String)]
        [Orleans.Id(1)]
        public string ServerAddress;

        [SerializeAs(DataType.UnsignedShort)]
        [Orleans.Id(2)]
        public ushort ServerPort;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(3)]
        public uint NextState;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsVarInt(ProtocolVersion, out _);
            bw.WriteAsString(ServerAddress);
            bw.WriteAsUnsignedShort(ServerPort);
            bw.WriteAsVarInt(NextState, out _);
        }

        public void Deserialize(ref SpanReader br)
        {
            ProtocolVersion = br.ReadAsVarInt(out _);
            ServerAddress = br.ReadAsString();
            ServerPort = br.ReadAsUnsignedShort();
            NextState = br.ReadAsVarInt(out _);
        }
    }
}

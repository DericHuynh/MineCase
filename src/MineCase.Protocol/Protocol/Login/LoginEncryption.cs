using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;
using Orleans.Serialization;

namespace MineCase.Protocol.Login
{
    [Packet(0x01)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class EncryptionRequest : IPacket
    {
        [SerializeAs(DataType.String)]
        [Orleans.Id(0)]
        public string ServerID;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public uint PublicKeyLength;

        [SerializeAs(DataType.ByteArray, ArrayLengthMember = nameof(PublicKeyLength))]
        [Orleans.Id(2)]
        public byte[] PublicKey;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(3)]
        public uint VerifyTokenLength;

        [SerializeAs(DataType.ByteArray, ArrayLengthMember = nameof(VerifyTokenLength))]
        [Orleans.Id(4)]
        public byte[] VerifyToken;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsString(ServerID);
            bw.WriteAsVarInt(PublicKeyLength, out _);
            bw.WriteAsByteArray(PublicKey);
            bw.WriteAsVarInt(VerifyTokenLength, out _);
            bw.WriteAsByteArray(VerifyToken);
        }

        public void Deserialize(ref SpanReader br)
        {
            ServerID = br.ReadAsString();
            PublicKeyLength = br.ReadAsVarInt(out _);
            PublicKey = br.ReadAsByteArray();
            VerifyTokenLength = br.ReadAsVarInt(out _);
            VerifyToken = br.ReadAsByteArray();
        }
    }

    [Packet(0x01)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class EncryptionResponse : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint SharedSecretLength;

        [SerializeAs(DataType.ByteArray, ArrayLengthMember = nameof(SharedSecretLength))]
        [Orleans.Id(1)]
        public byte[] SharedSecret;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(2)]
        public uint VerifyTokenLength;

        [SerializeAs(DataType.ByteArray, ArrayLengthMember = nameof(VerifyTokenLength))]
        [Orleans.Id(3)]
        public byte[] VerifyToken;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsVarInt(SharedSecretLength, out _);
            bw.WriteAsByteArray(SharedSecret);
            bw.WriteAsVarInt(VerifyTokenLength, out _);
            bw.WriteAsByteArray(VerifyToken);
        }

        public void Deserialize(ref SpanReader br)
        {
            SharedSecretLength = br.ReadAsVarInt(out _);
            SharedSecret = br.ReadAsByteArray();
            VerifyTokenLength = br.ReadAsVarInt(out _);
            VerifyToken = br.ReadAsByteArray();
        }
    }
}

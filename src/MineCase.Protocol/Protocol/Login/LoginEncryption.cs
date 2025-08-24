using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Login
{
    [Packet(0x01)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class EncryptionRequest : IPacket
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
    }

    [Packet(0x01)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class EncryptionResponse : IPacket
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
    }
}

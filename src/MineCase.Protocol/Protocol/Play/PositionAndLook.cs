using System.IO;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x36)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class ClientboundPositionAndLook : IPacket
    {
        [SerializeAs(DataType.Double)]
        [Orleans.Id(0)]
        public double X;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(1)]
        public double Y;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(2)]
        public double Z;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(3)]
        public float Yaw;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(4)]
        public float Pitch;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(5)]
        public byte Flags;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(6)]
        public uint TeleportId;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsDouble(X);
            bw.WriteAsDouble(Y);
            bw.WriteAsDouble(Z);
            bw.WriteAsFloat(Yaw);
            bw.WriteAsFloat(Pitch);
            bw.WriteAsByte(Flags);
            bw.WriteAsVarInt(TeleportId, out _);
        }

        public void Deserialize(ref SpanReader br)
        {
            X = br.ReadAsDouble();
            Y = br.ReadAsDouble();
            Z = br.ReadAsDouble();
            Yaw = br.ReadAsFloat();
            Pitch = br.ReadAsFloat();
            Flags = br.ReadAsByte();
            TeleportId = br.ReadAsVarInt(out _);
        }
    }

    [Packet(0x12)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class ServerboundPositionAndLook : IPacket
    {
        [SerializeAs(DataType.Double)]
        [Orleans.Id(0)]
        public double X;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(1)]
        public double FeetY;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(2)]
        public double Z;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(3)]
        public float Yaw;

        [SerializeAs(DataType.Float)]
        [Orleans.Id(4)]
        public float Pitch;

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(5)]
        public bool OnGround;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsDouble(X);
            bw.WriteAsDouble(FeetY);
            bw.WriteAsDouble(Z);
            bw.WriteAsFloat(Yaw);
            bw.WriteAsFloat(Pitch);
            bw.WriteAsBoolean(OnGround);
        }

        public void Deserialize(ref SpanReader br)
        {
            X = br.ReadAsDouble();
            FeetY = br.ReadAsDouble();
            Z = br.ReadAsDouble();
            Yaw = br.ReadAsFloat();
            Pitch = br.ReadAsFloat();
            OnGround = br.ReadAsBoolean();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x03)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class SpawnMob : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint EID;

        [SerializeAs(DataType.UUID)]
        [Orleans.Id(1)]
        public Guid EntityUUID;

        [SerializeAs(DataType.Byte)]
        [Orleans.Id(2)]
        public byte Type;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(3)]
        public double X;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(4)]
        public double Y;

        [SerializeAs(DataType.Double)]
        [Orleans.Id(5)]
        public double Z;

        [SerializeAs(DataType.Angle)]
        [Orleans.Id(6)]
        public Angle Pitch;

        [SerializeAs(DataType.Angle)]
        [Orleans.Id(7)]
        public Angle Yaw;

        [SerializeAs(DataType.Angle)]
        [Orleans.Id(8)]
        public Angle HeadPitch;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(9)]
        public short VelocityX;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(10)]
        public short VelocityY;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(11)]
        public short VelocityZ;

        [SerializeAs(DataType.ByteArray)]
        [Orleans.Id(12)]
        public byte[] Metadata;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsVarInt(EID, out _);
            bw.WriteAsUUID(EntityUUID);
            bw.WriteAsByte(Type);
            bw.WriteAsDouble(X);
            bw.WriteAsDouble(Y);
            bw.WriteAsDouble(Z);
            bw.WriteAsAngle(Pitch);
            bw.WriteAsAngle(Yaw);
            bw.WriteAsAngle(HeadPitch);
            bw.WriteAsShort(VelocityX);
            bw.WriteAsShort(VelocityY);
            bw.WriteAsShort(VelocityZ);
            bw.WriteAsByteArray(Metadata);
        }

        public void Deserialize(ref SpanReader br)
        {
            EID = br.ReadAsVarInt(out _);
            EntityUUID = br.ReadAsUUID();
            Type = br.ReadAsByte();
            X = br.ReadAsDouble();
            Y = br.ReadAsDouble();
            Z = br.ReadAsDouble();
            Pitch = br.ReadAsAngle();
            Yaw = br.ReadAsAngle();
            HeadPitch = br.ReadAsAngle();
            VelocityX = br.ReadAsShort();
            VelocityY = br.ReadAsShort();
            VelocityZ = br.ReadAsShort();
            Metadata = br.ReadAsByteArray();
        }
    }
}

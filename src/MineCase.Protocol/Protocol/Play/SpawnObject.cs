using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    [Packet(0x00)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class SpawnObject : IPacket
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint EID;

        [SerializeAs(DataType.UUID)]
        [Orleans.Id(1)]
        public Guid ObjectUUID;

        [SerializeAs(DataType.VarInt)]
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

        [SerializeAs(DataType.Int)]
        [Orleans.Id(8)]
        public int Data;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(9)]
        public short VelocityX;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(10)]
        public short VelocityY;

        [SerializeAs(DataType.Short)]
        [Orleans.Id(11)]
        public short VelocityZ;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsVarInt(EID, out _);
            bw.WriteAsUUID(ObjectUUID);
            bw.WriteAsByte(Type);
            bw.WriteAsDouble(X);
            bw.WriteAsDouble(Y);
            bw.WriteAsDouble(Z);
            bw.WriteAsAngle(Pitch);
            bw.WriteAsAngle(Yaw);
            bw.WriteAsInt(Data);
            bw.WriteAsShort(VelocityX);
            bw.WriteAsShort(VelocityY);
            bw.WriteAsShort(VelocityZ);
        }

        public void Deserialize(ref SpanReader br)
        {
            EID = br.ReadAsVarInt(out _);
            ObjectUUID = br.ReadAsUUID();
            Type = br.ReadAsByte();
            X = br.ReadAsDouble();
            Y = br.ReadAsDouble();
            Z = br.ReadAsDouble();
            Pitch = br.ReadAsAngle();
            Yaw = br.ReadAsAngle();
            Data = br.ReadAsInt();
            VelocityX = br.ReadAsShort();
            VelocityY = br.ReadAsShort();
            VelocityZ = br.ReadAsShort();
        }
    }
}

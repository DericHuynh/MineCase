using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase.Serialization
{
    public enum DataType
    {
        Boolean,
        Byte,
        UnsignedByte,
        Short,
        UnsignedShort,
        Int,
        Long,
        Float,
        Double,
        String,
        Chat,
        VarInt,
        VarLong,
        EntityMetadata,
        Slot,
        NBTTag,
        Position,
        Angle,
        UUID,
        ByteArray,
        IntArray,
        NbtArray,
        VarIntArray,
        SlotArray,
        Array
    }

    /// <summary>
    /// Im pretty sure this has no effect on the code and is meant to annotate the contract for the developer to implement serializing/deserialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class SerializeAsAttribute : Attribute
    {
        public DataType DataType { get; }

        public string ArrayLengthMember { get; set; }

        public SerializeAsAttribute(DataType dataType)
        {
            DataType = dataType;
        }
    }
}

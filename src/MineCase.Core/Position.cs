using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MineCase
{
    [Orleans.GenerateSerializer]
    public struct Position : IEquatable<Position>
    {
        [Orleans.Id(0)]
        public int X { get; set; }

        [Orleans.Id(1)]
        public int Y { get; set; }

        [Orleans.Id(2)]
        public int Z { get; set; }

        public static implicit operator Vector3(Position position)
        {
            return new Vector3(position.X, position.Y, position.Z);
        }

        public override bool Equals(object obj)
        {
            return obj is Position && Equals((Position)obj);
        }

        public bool Equals(Position other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z;
        }

        public override int GetHashCode()
        {
            var hashCode = -307843816;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Position position1, Position position2)
        {
            return position1.Equals(position2);
        }

        public static bool operator !=(Position position1, Position position2)
        {
            return !(position1 == position2);
        }
    }
}

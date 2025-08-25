using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace MineCase.Server.OrleansSerializer
{
    [GenerateSerializer]
    public struct Vector3dSurrogate
    {
        [Id(0)]
        public float X;

        [Id(1)]
        public float Y;

        [Id(2)]
        public float Z;
    }

    // This is a converter that converts between the surrogate and the foreign type.
    [RegisterConverter]
    public sealed class Vector3SurrogateConverter :
        IConverter<Vector3, Vector3dSurrogate>
    {
        public Vector3 ConvertFromSurrogate(
            in Vector3dSurrogate surrogate) =>
            new (surrogate.X, surrogate.Y, surrogate.Z);

        public Vector3dSurrogate ConvertToSurrogate(
            in Vector3 value) =>
            new ()
            {
                X = value.X,
                Y = value.Y,
                Z = value.Z
            };
    }
}

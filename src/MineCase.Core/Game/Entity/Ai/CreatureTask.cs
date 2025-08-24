using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MineCase.Server.World.EntitySpawner
{
    [Orleans.GenerateSerializer]
    public struct CreatureTask
    {
        [Orleans.Id(0)]
        public uint Health { get; set; }

        [Orleans.Id(1)]
        public Vector3 Position { get; set; }

        [Orleans.Id(2)]
        public byte Pitch { get; set; }

        [Orleans.Id(3)]
        public byte Yaw { get; set; }

        [Orleans.Id(4)]
        public bool OnGround { get; set; }
    }
}

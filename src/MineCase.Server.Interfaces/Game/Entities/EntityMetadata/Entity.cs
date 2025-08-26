using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase.Server.Game.Entities.EntityMetadata
{
    [Orleans.GenerateSerializer]
    public class Entity
    {
        [Orleans.Id(0)]
        public bool OnFire { get; set; }

        [Orleans.Id(1)]
        public bool Crouched { get; set; }

        [Orleans.Id(2)]
        public bool Sprinting { get; set; }

        [Orleans.Id(3)]
        public bool Invisible { get; set; }

        [Orleans.Id(4)]
        public bool GlowingEffect { get; set; }

        [Orleans.Id(5)]
        public bool FlyingWithElytra { get; set; }

        [Orleans.Id(6)]
        public int Air { get; set; } = 300;

        [Orleans.Id(7)]
        public string CustomName { get; set; } = string.Empty;

        [Orleans.Id(8)]
        public bool IsCustomNameVisible { get; set; }

        [Orleans.Id(9)]
        public bool IsSilent { get; set; }

        [Orleans.Id(10)]
        public bool NoGravity { get; set; }
    }
}

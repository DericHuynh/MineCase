using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase.Block.Material
{
    [Orleans.GenerateSerializer]
    public class Material : IEquatable<Material>
    {
        [Orleans.Id(0)]
        public MaterialColor Color { get; set; }

        [Orleans.Id(1)]
        public PushReaction PushReaction { get; set; } = PushReaction.Normal;

        [Orleans.Id(2)]
        public bool BlocksMovement { get; set; } = true;

        [Orleans.Id(3)]
        public bool Flammable { get; set; } = false;

        [Orleans.Id(4)]
        public bool RequiresNoTool { get; set; } = true;

        [Orleans.Id(5)]
        public bool Liquid { get; set; } = false;

        [Orleans.Id(6)]
        public bool Opaque { get; set; } = true;

        [Orleans.Id(7)]
        public bool Replaceable { get; set; } = false;

        [Orleans.Id(8)]
        public bool Solid { get; set; } = true;

        public override bool Equals(object obj)
        {
            return obj is Material && Equals((Material)obj);
        }

        public bool Equals(Material other)
        {
            return Color == other.Color &&
                   PushReaction == other.PushReaction &&
                   BlocksMovement == other.BlocksMovement &&
                   Flammable == other.Flammable &&
                   RequiresNoTool == other.RequiresNoTool &&
                   Liquid == other.Liquid &&
                   Opaque == other.Opaque &&
                   Replaceable == other.Replaceable &&
                   Solid == other.Solid;
        }

        public override int GetHashCode()
        {
            var hashCode = -81208087;
            hashCode = hashCode * -1521134295 + Color.GetHashCode();
            hashCode = hashCode * -1521134295 + PushReaction.GetHashCode();
            hashCode = hashCode * -1521134295 + BlocksMovement.GetHashCode();
            hashCode = hashCode * -1521134295 + Flammable.GetHashCode();
            hashCode = hashCode * -1521134295 + RequiresNoTool.GetHashCode();
            hashCode = hashCode * -1521134295 + Liquid.GetHashCode();
            hashCode = hashCode * -1521134295 + Opaque.GetHashCode();
            hashCode = hashCode * -1521134295 + Replaceable.GetHashCode();
            hashCode = hashCode * -1521134295 + Solid.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Material state1, Material state2)
        {
            return state1.Equals(state2);
        }

        public static bool operator !=(Material state1, Material state2)
        {
            return !(state1 == state2);
        }
    }
}

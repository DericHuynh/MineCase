using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.World;
using MineCase.World;

namespace MineCase.Server.Components
{
    internal class WorldComponent : Component
    {
        public static readonly DependencyProperty<IWorld> WorldProperty =
            DependencyProperty.Register<IWorld>("World", typeof(WorldComponent));

        public WorldComponent(string name = "world")
            : base(name)
        {
        }

        public void SetWorld(IWorld value) =>
            AttachedEntity.SetLocalValue(WorldProperty, value);
    }

    public static class WorldComponentExtensions
    {
        public static IWorld GetWorld(this Entity d) =>
            d.GetValue(WorldComponent.WorldProperty);

        public static bool TryGetWorld(this Entity d, out IWorld value) =>
            d.TryGetLocalValue(WorldComponent.WorldProperty, out value);
    }
}

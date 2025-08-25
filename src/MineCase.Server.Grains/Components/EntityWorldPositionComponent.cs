using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.World;

namespace MineCase.Server.Components
{
    internal class EntityWorldPositionComponent : Component
    {
        public static readonly DependencyProperty<EntityWorldPos> EntityWorldPositionProperty =
            DependencyProperty.Register<EntityWorldPos>("EntityWorldPosition", typeof(EntityWorldPositionComponent));

        public EntityWorldPositionComponent(string name = "entityWorldPosition")
            : base(name)
        {
        }

        public void SetPosition(EntityWorldPos entityWorldPos)
            => AttachedEntity.SetLocalValue(EntityWorldPositionProperty, entityWorldPos);
    }

    public static class EntityWorldPositionComponentExtensions
    {
        public static EntityWorldPos GetEntityWorldPosition(this Entity d) =>
            d.GetValue(EntityWorldPositionComponent.EntityWorldPositionProperty);

        public static bool TryGetEntityWorldPosition(this Entity d, out EntityWorldPos value) =>
            d.TryGetLocalValue(EntityWorldPositionComponent.EntityWorldPositionProperty, out value);
    }
}

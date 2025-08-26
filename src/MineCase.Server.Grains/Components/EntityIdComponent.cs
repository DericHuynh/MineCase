using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Game.Entities.Components;

namespace MineCase.Server.Components
{
    internal class EntityIdComponent : Component, IHandle<SpawnEntity>
    {
        public static readonly DependencyProperty<int> EntityIdProperty =
            DependencyProperty.Register<int>("EntityId", typeof(EntityIdComponent));

        public int EntityId => AttachedEntity.GetValue(EntityIdProperty);

        public EntityIdComponent(string name = "entityId")
            : base(name)
        {
        }

        Task IHandle<SpawnEntity>.Handle(SpawnEntity message)
        {
            AttachedEntity.SetLocalValue(EntityIdProperty, message.EntityId);
            return Task.CompletedTask;
        }
    }
}

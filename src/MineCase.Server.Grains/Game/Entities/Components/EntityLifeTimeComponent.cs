using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Components;
using MineCase.Server.Game.BlockEntities;

namespace MineCase.Server.Game.Entities.Components
{
    internal class EntityLifeTimeComponent : Component<EntityGrain>, IHandle<SpawnEntity>, IHandle<DestroyEntity>
    {
        public EntityLifeTimeComponent(string name = "entityLifeTime")
            : base(name)
        {
        }

        Task IHandle<SpawnEntity>.Handle(SpawnEntity message)
        {
            AttachedEntity.GetComponent<WorldComponent>().SetWorld(message.World);
            AttachedEntity.GetComponent<EntityWorldPositionComponent>().SetPosition(message.Position);
            var lookComponent = AttachedEntity.GetComponent<EntityLookComponent>();
            lookComponent.SetPitch(message.Pitch);
            lookComponent.SetHeadYaw(message.Yaw);
            lookComponent.SetYaw(message.Yaw);
            return Task.CompletedTask;
        }

        async Task IHandle<DestroyEntity>.Handle(DestroyEntity message)
        {
            await AttachedEntity.Tell(Disable.Default);
            AttachedEntity.QueueOperation(() =>
            {
                AttachedEntity.Destroy();
                return Task.CompletedTask;
            });
        }
    }
}

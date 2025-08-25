using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Components;

namespace MineCase.Server.Game.BlockEntities.Components
{
    internal class BlockEntityLiftTimeComponent : Component<BlockEntityGrain>, IHandle<SpawnBlockEntity>, IHandle<DestroyBlockEntity>
    {
        public BlockEntityLiftTimeComponent(string name = "blockEntityLifeTime")
            : base(name)
        {
        }

        Task IHandle<SpawnBlockEntity>.Handle(SpawnBlockEntity message)
        {
            AttachedEntity.GetComponent<WorldComponent>().SetWorld(message.World);
            AttachedEntity.GetComponent<BlockWorldPositionComponent>().SetBlockWorldPosition(message.Position);
            AttachedEntity.QueueOperation(async () =>
            {
                await AttachedEntity.Tell(Enable.Default);
                if (AttachedEntity.ValueStorage.IsDirty)
                    await AttachedEntity.WriteStateAsync();
            });
            return Task.CompletedTask;
        }

        Task IHandle<DestroyBlockEntity>.Handle(DestroyBlockEntity message)
        {
            return AttachedEntity.Tell(Disable.Default);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Components;
using MineCase.Server.Game.Windows;
using MineCase.Server.Network.Play;
using Orleans;

namespace MineCase.Server.Game.Entities.Components
{
    internal class InventoryComponent : Component<PlayerGrain>, IHandle<AskInventoryWindow, IInventoryWindow>, IHandle<AskCollectionResult, Slot>
    {
        public InventoryComponent(string name = "inventory")
            : base(name)
        {
        }

        public IInventoryWindow GetInventoryWindow() =>
            GrainFactory.GetGrain<IInventoryWindow>(AttachedEntity.GetPrimaryKey());

        Task<IInventoryWindow> IHandle<AskInventoryWindow, IInventoryWindow>.Handle(AskInventoryWindow message) =>
            Task.FromResult(GetInventoryWindow());

        async Task<Slot> IHandle<AskCollectionResult, Slot>.Handle(AskCollectionResult message)
        {
            var after = await GetInventoryWindow().DistributeStack(AttachedEntity, message.Slot);
            if (after.ItemCount != message.Slot.ItemCount)
            {
                await AttachedEntity.GetComponent<ChunkEventBroadcastComponent>().GetGenerator()
                    .CollectItem(await message.Source.GetEntityId(), AttachedEntity.EntityId, message.Slot.ItemCount - after.ItemCount);
            }

            return after;
        }
    }
}

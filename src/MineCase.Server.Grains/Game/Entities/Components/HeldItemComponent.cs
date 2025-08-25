using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;

namespace MineCase.Server.Game.Entities.Components
{
    internal class HeldItemComponent : Component<PlayerGrain>, IHandle<SetHeldItemIndex>, IHandle<AskHeldItem, (int Index, Slot Slot)>, IHandle<SetHeldItem>
    {
        public static readonly DependencyProperty<int> HeldItemIndexProperty =
            DependencyProperty.Register("HeldItemIndex", typeof(HeldItemComponent), new PropertyMetadata<int>(0));

        public int HeldItemIndex => AttachedEntity.GetValue(HeldItemIndexProperty);

        public HeldItemComponent(string name = "heldItem")
            : base(name)
        {
        }

        public async Task<(int Index, Slot Slot)> GetHeldItem()
        {
            var inventory = AttachedEntity.GetComponent<InventoryComponent>().GetInventoryWindow();
            var index = await inventory.GetHotbarGlobalIndex(AttachedEntity, HeldItemIndex);
            return (index, await inventory.GetSlot(AttachedEntity, index));
        }

        public void SetHeldItemIndex(int index) =>
            AttachedEntity.SetLocalValue(HeldItemIndexProperty, index);

        Task IHandle<SetHeldItemIndex>.Handle(SetHeldItemIndex message)
        {
            SetHeldItemIndex(message.Index);
            return Task.CompletedTask;
        }

        Task<(int Index, Slot Slot)> IHandle<AskHeldItem, (int Index, Slot Slot)>.Handle(AskHeldItem message) =>
            GetHeldItem();

        async Task IHandle<SetHeldItem>.Handle(SetHeldItem message)
        {
            var inventory = AttachedEntity.GetComponent<InventoryComponent>().GetInventoryWindow();
            var index = await inventory.GetHotbarGlobalIndex(AttachedEntity, HeldItemIndex);
            await inventory.SetSlot(AttachedEntity, index, message.Slot);
        }
    }
}

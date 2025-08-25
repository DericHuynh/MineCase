using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Components;
using MineCase.Server.World;

namespace MineCase.Server.Game.Entities.Components
{
    internal class DiscoveryRegisterComponent : Component<EntityGrain>
    {
        public DiscoveryRegisterComponent(string name = "discoveryRegister")
            : base(name)
        {
        }

        protected override void OnAttached()
        {
            AttachedEntity.GetComponent<AddressByPartitionKeyComponent>()
                .KeyChanged += AddressByPartitionKeyChanged;
            AttachedEntity.RegisterPropertyChangedHandler(IsEnabledComponent.IsEnabledProperty, OnIsEnabledChanged);
            AttachedEntity.QueueOperation(TrySubscribe);
        }

        protected override void OnDetached()
        {
            AttachedEntity.GetComponent<AddressByPartitionKeyComponent>()
                .KeyChanged -= AddressByPartitionKeyChanged;
            AttachedEntity.QueueOperation(TryUnsubscribe);
        }

        private void AddressByPartitionKeyChanged(object sender, (string OldKey, string NewKey) e)
        {
            AttachedEntity.QueueOperation(async () =>
            {
                if (!string.IsNullOrEmpty(e.OldKey))
                    await GrainFactory.GetGrain<IWorldPartition>(e.OldKey).UnsubscribeDiscovery(AttachedEntity);
                await TrySubscribe();
            });
        }

        private void OnIsEnabledChanged(object sender, PropertyChangedEventArgs<bool> e)
        {
            if (e.NewValue)
                AttachedEntity.QueueOperation(TrySubscribe);
            else
                AttachedEntity.QueueOperation(TryUnsubscribe);
        }

        private async Task TrySubscribe()
        {
            if (AttachedEntity.GetValue(IsEnabledComponent.IsEnabledProperty))
            {
                var key = AttachedEntity.GetAddressByPartitionKey();
                if (!string.IsNullOrEmpty(key))
                    await GrainFactory.GetGrain<IWorldPartition>(key).SubscribeDiscovery(AttachedEntity);
            }
        }

        private async Task TryUnsubscribe()
        {
            var key = AttachedEntity.GetAddressByPartitionKey();
            if (!string.IsNullOrEmpty(key))
                await GrainFactory.GetGrain<IWorldPartition>(key).UnsubscribeDiscovery(AttachedEntity);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.World;
using MineCase.World;
using Orleans;
using Orleans.Concurrency;

namespace MineCase.Server.Components
{
    internal class GameTickComponent : Component, IHandle<GameTick>
    {
        public event AsyncEventHandler<GameTickArgs> Tick;

        public GameTickComponent(string name = "gameTick")
            : base(name)
        {
        }

        protected override void OnAttached()
        {
            AttachedEntity.GetComponent<AddressByPartitionKeyComponent>()
                .KeyChanged += OnAddressByPartitionKeyChanged;
            AttachedEntity.RegisterPropertyChangedHandler(IsEnabledComponent.IsEnabledProperty, OnIsEnabledChanged);
            AttachedEntity.QueueOperation(TrySubscribe);
        }

        protected override void OnDetached()
        {
            AttachedEntity.GetComponent<AddressByPartitionKeyComponent>()
                .KeyChanged -= OnAddressByPartitionKeyChanged;
            AttachedEntity.QueueOperation(TryUnsubscribe);
        }

        private void OnAddressByPartitionKeyChanged(object sender, (string OldKey, string NewKey) e)
        {
            AttachedEntity.QueueOperation(async () =>
            {
                if (!string.IsNullOrEmpty(e.OldKey))
                    await GrainFactory.GetGrain<ITickEmitter>(e.OldKey).Unsubscribe(AttachedEntity);
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

        public Task OnGameTick(GameTickArgs e)
        {
            return Tick.InvokeSerial(this, e);
        }

        Task IHandle<GameTick>.Handle(GameTick message)
        {
            return OnGameTick(message.Args);
        }

        private async Task TrySubscribe()
        {
            if (AttachedEntity.GetValue(IsEnabledComponent.IsEnabledProperty))
            {
                var key = AttachedEntity.GetAddressByPartitionKey();
                if (!string.IsNullOrEmpty(key))
                    await GrainFactory.GetGrain<ITickEmitter>(key).Subscribe(AttachedEntity);
            }
        }

        private async Task TryUnsubscribe()
        {
            var key = AttachedEntity.GetAddressByPartitionKey();
            if (!string.IsNullOrEmpty(key))
                await GrainFactory.GetGrain<ITickEmitter>(key).Unsubscribe(AttachedEntity);
        }
    }
}

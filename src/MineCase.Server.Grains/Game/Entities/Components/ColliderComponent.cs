using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Graphics;
using MineCase.Server.Components;
using MineCase.Server.World;
using MineCase.World;

namespace MineCase.Server.Game.Entities.Components
{
    internal class ColliderComponent : Component<EntityGrain>
    {
        public static readonly DependencyProperty<Shape> ColliderShapeProperty =
            DependencyProperty.Register<Shape>("ColliderShape", typeof(ColliderComponent), new PropertyMetadata<Shape>(null, OnColliderShapeChanged));

        public Shape ColliderShape => AttachedEntity.GetValue(ColliderShapeProperty);

        public ColliderComponent(string name = "collider")
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
                var shape = ColliderShape;
                if (!string.IsNullOrEmpty(e.OldKey))
                    await GrainFactory.GetGrain<ICollectableFinder>(e.OldKey).UnregisterCollider(AttachedEntity);
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

        private void OnColliderShapeChanged(PropertyChangedEventArgs<Shape> e)
        {
            var shape = ColliderShape;
            var key = AttachedEntity.GetValue(AddressByPartitionKeyComponent.AddressByPartitionKeyProperty);
            if (shape != null)
                AttachedEntity.QueueOperation(() => GrainFactory.GetGrain<ICollectableFinder>(key).RegisterCollider(AttachedEntity, shape));
            else
                AttachedEntity.QueueOperation(TrySubscribe);
        }

        private static void OnColliderShapeChanged(object sender, PropertyChangedEventArgs<Shape> e)
        {
            var component = ((Entity)sender).GetComponent<ColliderComponent>();
            component.OnColliderShapeChanged(e);
        }

        public void SetColliderShape(Shape value) =>
            AttachedEntity.SetLocalValue(ColliderShapeProperty, value);

        private async Task TrySubscribe()
        {
            if (AttachedEntity.GetValue(IsEnabledComponent.IsEnabledProperty))
            {
                var key = AttachedEntity.GetAddressByPartitionKey();
                var shape = ColliderShape;
                if (!string.IsNullOrEmpty(key) && shape != null)
                    await GrainFactory.GetGrain<ICollectableFinder>(key).RegisterCollider(AttachedEntity, ColliderShape);
            }
        }

        private async Task TryUnsubscribe()
        {
            var key = AttachedEntity.GetAddressByPartitionKey();
            if (!string.IsNullOrEmpty(key))
                await GrainFactory.GetGrain<ICollectableFinder>(key).UnregisterCollider(AttachedEntity);
        }
    }
}

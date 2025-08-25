using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Game.Entities.Components;
using MineCase.Server.Game.Windows;
using MineCase.World;
using Orleans;
using Orleans.Concurrency;

namespace MineCase.Server.Game.BlockEntities.Components
{
    internal class ChestComponent : Component<BlockEntityGrain>, IHandle<NeighborEntityChanged>, IHandle<DestroyBlockEntity>, IHandle<UseBy>
    {
        public static readonly DependencyProperty<IBlockEntity> NeighborEntityProperty =
            DependencyProperty.Register(nameof(NeighborEntity), typeof(ChestComponent), new PropertyMetadata<IBlockEntity>(null, OnNeighborEntityChanged));

        public static readonly DependencyProperty<IChestWindow> ChestWindowProperty =
            DependencyProperty.Register<IChestWindow>(nameof(ChestWindow), typeof(ChestComponent));

        public IBlockEntity NeighborEntity => AttachedEntity.GetValue(NeighborEntityProperty);

        public IChestWindow ChestWindow => AttachedEntity.GetValue(ChestWindowProperty);

        public ChestComponent(string name = "chest")
            : base(name)
        {
        }

        Task IHandle<NeighborEntityChanged>.Handle(NeighborEntityChanged message)
        {
            AttachedEntity.SetLocalValue(NeighborEntityProperty, message.Entity);
            return Task.CompletedTask;
        }

        private static void OnNeighborEntityChanged(object sender, PropertyChangedEventArgs<IBlockEntity> e)
        {
            var component = ((Entity)sender).GetComponent<ChestComponent>();
            var window = component?.ChestWindow;
            if (window == null) return;
            if (e.NewValue == null)
            {
                component.AttachedEntity.QueueOperation(async () =>
                {
                    await window.Destroy();
                    await window.SetEntities(new[] { component.AttachedEntity.AsReference<IEntity>() }.AsImmutable());
                });
            }
            else
            {
                component.AttachedEntity.QueueOperation(async () =>
                {
                    await window.Destroy();
                    await window.SetEntities(new[] { component.AttachedEntity.AsReference<IEntity>(), e.NewValue.AsReference<IEntity>() }.AsImmutable());
                });
            }
        }

        async Task IHandle<DestroyBlockEntity>.Handle(DestroyBlockEntity message)
        {
            if (ChestWindow != null)
                await ChestWindow.Destroy();
        }

        async Task IHandle<UseBy>.Handle(UseBy message)
        {
            var masterEntity = await FindMasterEntity(NeighborEntity);
            if (object.Equals(masterEntity, AttachedEntity.AsReference<IBlockEntity>()))
            {
                if (ChestWindow == null)
                    AttachedEntity.SetLocalValue(ChestWindowProperty, GrainFactory.GetGrain<IChestWindow>(Guid.NewGuid()));

                await ChestWindow.SetEntities((NeighborEntity == null ?
                    new[] { AttachedEntity.AsReference<IEntity>() } :
                    new[] { AttachedEntity.AsReference<IEntity>(), NeighborEntity }).AsImmutable());
                await message.Entity.Tell(new OpenWindow { Window = ChestWindow });
            }
            else
            {
                await masterEntity.Tell(message);
            }
        }

        private async Task<IBlockEntity> FindMasterEntity(IBlockEntity neighborEntity)
        {
            if (NeighborEntity == null)
                return AttachedEntity.AsReference<IBlockEntity>();

            async Task<(IBlockEntity Entity, BlockWorldPos Position)> GetPosition(IBlockEntity entity) =>
                (entity, await entity.GetPosition());

            // 按 X, Z 排序取最小
            return (from e in await Task.WhenAll(new[] { GetPosition(AttachedEntity.AsReference<IBlockEntity>()), GetPosition(NeighborEntity) })
                    orderby e.Position.X, e.Position.Z
                    select e.Entity).First();
        }
    }
}

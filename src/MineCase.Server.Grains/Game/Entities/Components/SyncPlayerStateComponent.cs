using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Graphics;
using MineCase.Server.Components;
using MineCase.Server.Network.Play;
using MineCase.Server.User;
using MineCase.World;
using Orleans;

namespace MineCase.Server.Game.Entities.Components
{
    [Orleans.GenerateSerializer]
    internal class SyncPlayerStateComponent : Component<PlayerGrain>, IHandle<PlayerLoggedIn>, IHandle<BindToUser>
    {
        [Id(0)]
        private IUser _user;
        [Id(1)]
        private bool _isHandlersInstalled;

        public SyncPlayerStateComponent(string name = "syncPlayerState")
            : base(name)
        {
            _isHandlersInstalled = false;
        }

        async Task IHandle<PlayerLoggedIn>.Handle(PlayerLoggedIn message)
        {
            var generator = AttachedEntity.GetComponent<ClientboundPacketComponent>().GetGenerator();

            // PositionAndLook
            var position = AttachedEntity.GetEntityWorldPosition();
            var lookComponent = AttachedEntity.GetComponent<EntityLookComponent>();
            await generator.PositionAndLook(position.X, position.Y, position.Z, lookComponent.Yaw, lookComponent.Pitch, 0, AttachedEntity.GetComponent<TeleportComponent>().StartNew());

            // Update view position
            var chunkPos = position.ToChunkWorldPos();
            await generator.UpdateViewPosition(chunkPos.X, chunkPos.Z);

            // Health
            var healthComponent = AttachedEntity.GetComponent<HealthComponent>();
            var foodComponent = AttachedEntity.GetComponent<FoodComponent>();
            await generator.UpdateHealth(healthComponent.Health, healthComponent.MaxHealth, foodComponent.Food, foodComponent.MaxFood, foodComponent.FoodSaturation);

            // Experience
            var expComponent = AttachedEntity.GetComponent<ExperienceComponent>();
            await generator.SetExperience(expComponent.ExperienceBar, expComponent.Level, expComponent.TotalExperience);

            // Inventory
            var slots = await AttachedEntity.GetComponent<InventoryComponent>().GetInventoryWindow().GetSlots(AttachedEntity);
            await generator.WindowItems(0, slots);

            if (!_isHandlersInstalled)
            {
                InstallPropertyChangedHandlers();
                _isHandlersInstalled = true;
            }
        }

        private void InstallPropertyChangedHandlers()
        {
            AttachedEntity.RegisterPropertyChangedHandler(DraggedSlotComponent.DraggedSlotProperty, OnDraggedSlotChanged);
            AttachedEntity.RegisterPropertyChangedHandler(EntityWorldPositionComponent.EntityWorldPositionProperty, OnEntityWorldPositionChanged);
            AttachedEntity.RegisterPropertyChangedHandler(EntityLookComponent.HeadYawProperty, OnEntityHeadYawChanged);
            AttachedEntity.RegisterPropertyChangedHandler(EntityLookComponent.PitchProperty, OnEntityPitchChanged);
            AttachedEntity.RegisterPropertyChangedHandler(EntityLookComponent.YawProperty, OnEntityYawChanged);
            AttachedEntity.RegisterPropertyChangedHandler(HealthComponent.HealthProperty, OnEntityHealthChanged);
        }

        [Id(2)]
        private ChunkEventBroadcastComponent _broadcastComponent;

        private void OnEntityHeadYawChanged(object sender, PropertyChangedEventArgs<float> e)
        {
            _broadcastComponent = _broadcastComponent ?? AttachedEntity.GetComponent<ChunkEventBroadcastComponent>();
            _broadcastComponent.GetGenerator(AttachedEntity)
                .EntityHeadLook(
                AttachedEntity.EntityId,
                GetAngle(e.NewValue));
        }

        private void OnEntityYawChanged(object sender, PropertyChangedEventArgs<float> e)
        {
            var yaw = AttachedEntity.GetValue(EntityLookComponent.YawProperty);
            var pitch = AttachedEntity.GetValue(EntityLookComponent.PitchProperty);
            _broadcastComponent = _broadcastComponent ?? AttachedEntity.GetComponent<ChunkEventBroadcastComponent>();
            _broadcastComponent.GetGenerator(AttachedEntity)
                .EntityLook(
                AttachedEntity.EntityId,
                GetAngle(yaw),
                GetAngle(pitch),
                AttachedEntity.GetValue(EntityOnGroundComponent.IsOnGroundProperty));
        }

        private void OnEntityPitchChanged(object sender, PropertyChangedEventArgs<float> e)
        {
            var yaw = AttachedEntity.GetValue(EntityLookComponent.YawProperty);
            var pitch = AttachedEntity.GetValue(EntityLookComponent.PitchProperty);
            _broadcastComponent = _broadcastComponent ?? AttachedEntity.GetComponent<ChunkEventBroadcastComponent>();
            _broadcastComponent.GetGenerator(AttachedEntity)
                .EntityLook(
                AttachedEntity.EntityId,
                GetAngle(yaw),
                GetAngle(pitch),
                AttachedEntity.GetValue(EntityOnGroundComponent.IsOnGroundProperty));
        }

        private void OnEntityWorldPositionChanged(object sender, PropertyChangedEventArgs<EntityWorldPos> e)
        {
            var generator = AttachedEntity.GetComponent<ClientboundPacketComponent>().GetGenerator();

            // Update Collider
            var pos = e.NewValue;
            var box = new Cuboid(new Point3d(pos.X, pos.Z, pos.Y), new Size(0.6f, 0.6f, 1.75f));
            AttachedEntity.SetLocalValue(ColliderComponent.ColliderShapeProperty, box);

            // Check if we need to send UpdateViewPosition packet. If the player walk cross chunk borders, send it.
            var oldChunkPos = e.OldValue.ToChunkWorldPos();
            var newChunkPos = e.NewValue.ToChunkWorldPos();
            if (oldChunkPos != newChunkPos)
            {
                generator.UpdateViewPosition(newChunkPos.X, newChunkPos.Z);
            }

            // Broadcast to trackers
            _broadcastComponent = _broadcastComponent ?? AttachedEntity.GetComponent<ChunkEventBroadcastComponent>();
            _broadcastComponent.GetGenerator(AttachedEntity)
                .EntityRelativeMove(
                AttachedEntity.EntityId,
                GetDelta(e.OldValue.X, e.NewValue.X),
                GetDelta(e.OldValue.Y, e.NewValue.Y),
                GetDelta(e.OldValue.Z, e.NewValue.Z),
                AttachedEntity.GetValue(EntityOnGroundComponent.IsOnGroundProperty));
        }

        private void OnEntityHealthChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            var generator = AttachedEntity.GetComponent<ClientboundPacketComponent>().GetGenerator();
            var healthComponent = AttachedEntity.GetComponent<HealthComponent>();
            var foodComponent = AttachedEntity.GetComponent<FoodComponent>();
            generator.UpdateHealth(healthComponent.Health, healthComponent.MaxHealth, foodComponent.Food, foodComponent.MaxFood, foodComponent.FoodSaturation);
            if (healthComponent.Health < 0)
            {
                AttachedEntity.SetLocalValue(DeathComponent.IsDeathProperty, true);
            }
        }

        private static short GetDelta(float before, float after)
        {
            return (short)((after * 32 - before * 32) * 128);
        }

        private static byte GetAngle(float degree)
        {
            return (byte)(degree * 256 / 360);
        }

        private void OnDraggedSlotChanged(object sender, PropertyChangedEventArgs<Slot> e)
        {
            AttachedEntity.QueueOperation(() => AttachedEntity.GetComponent<ClientboundPacketComponent>().GetGenerator()
                .SetSlot(0xFF, 0, e.NewValue));
        }

        async Task IHandle<BindToUser>.Handle(BindToUser message)
        {
            AttachedEntity.GetComponent<SlotContainerComponent>().SlotChanged -= InventorySlotChanged;

            _user = message.User;
            AttachedEntity.GetComponent<NameComponent>().SetName(await message.User.GetName());
            AttachedEntity.GetComponent<GameModeComponent>().SetGameMode(await message.User.GetGameMode());
            AttachedEntity.GetComponent<SlotContainerComponent>().SetSlots(await message.User.GetInventorySlots());

            AttachedEntity.GetComponent<SlotContainerComponent>().SlotChanged += InventorySlotChanged;
        }

        private void InventorySlotChanged(object sender, (int Index, Slot Slot) e)
        {
            AttachedEntity.QueueOperation(() => _user.SetInventorySlot(e.Index, e.Slot));
        }
    }
}

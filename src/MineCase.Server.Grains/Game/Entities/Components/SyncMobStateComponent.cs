using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Components;
using MineCase.Server.Network.Play;
using MineCase.World;

namespace MineCase.Server.Game.Entities.Components
{
    internal class SyncMobStateComponent : Component<EntityGrain>, IHandle<SpawnMob>
    {
        public SyncMobStateComponent(string name = "syncPlayerState")
            : base(name)
        {
        }

        protected override void OnAttached()
        {
            if (AttachedEntity.GetValue(IsEnabledComponent.IsEnabledProperty))
                InstallPropertyChangedHandlers();
        }

        Task IHandle<SpawnMob>.Handle(SpawnMob message)
        {
            InstallPropertyChangedHandlers();
            return Task.CompletedTask;
        }

        private void InstallPropertyChangedHandlers()
        {
            AttachedEntity.RegisterPropertyChangedHandler(EntityLookComponent.HeadYawProperty, OnHeadYawChanged);
            AttachedEntity.RegisterPropertyChangedHandler(EntityLookComponent.YawProperty, OnYawChanged);
            AttachedEntity.RegisterPropertyChangedHandler(EntityLookComponent.PitchProperty, OnPitchChanged);
            AttachedEntity.RegisterPropertyChangedHandler(EntityWorldPositionComponent.EntityWorldPositionProperty, OnPositionChanged);
        }

        private void OnHeadYawChanged(object sender, PropertyChangedEventArgs<float> e)
        {
            uint eid = AttachedEntity.GetValue(EntityIdComponent.EntityIdProperty);
            byte headyaw = (byte)(AttachedEntity.GetValue(EntityLookComponent.HeadYawProperty) / 360 * 255);
            AttachedEntity.QueueOperation(() => AttachedEntity.GetComponent<ChunkEventBroadcastComponent>().GetGenerator().EntityHeadLook(eid, headyaw));
        }

        private void OnYawChanged(object sender, PropertyChangedEventArgs<float> e)
        {
            /*
            uint eid = AttachedEntity.GetValue(EntityIdComponent.EntityIdProperty);
            byte yaw = (byte)(AttachedEntity.GetValue(EntityLookComponent.YawProperty) / 360 * 255);
            byte pitch = (byte)(AttachedEntity.GetValue(EntityLookComponent.PitchProperty) / 360 * 255);
            bool onGround = AttachedEntity.GetValue(EntityOnGroundComponent.IsOnGroundProperty);

            // return AttachedEntity.GetComponent<ChunkEventBroadcastComponent>().GetGenerator().EntityLook(eid, yaw, pitch, onGround);
            return AttachedEntity.GetComponent<ChunkEventBroadcastComponent>().GetGenerator().EntityLookAndRelativeMove(eid, 0, 0, 0, yaw, pitch, onGround);
            */
        }

        private void OnPitchChanged(object sender, PropertyChangedEventArgs<float> e)
        {
            uint eid = AttachedEntity.GetValue(EntityIdComponent.EntityIdProperty);
            byte yaw = (byte)(AttachedEntity.GetValue(EntityLookComponent.YawProperty) / 360 * 255);
            byte pitch = (byte)(AttachedEntity.GetValue(EntityLookComponent.PitchProperty) / 360 * 255);
            bool onGround = AttachedEntity.GetValue(EntityOnGroundComponent.IsOnGroundProperty);

            // return AttachedEntity.GetComponent<ChunkEventBroadcastComponent>().GetGenerator().EntityLook(eid, yaw, pitch, onGround);
            AttachedEntity.QueueOperation(() => AttachedEntity.GetComponent<ChunkEventBroadcastComponent>().GetGenerator().EntityLook(eid, yaw, pitch, onGround));
        }

        private void OnPositionChanged(object sender, PropertyChangedEventArgs<EntityWorldPos> e)
        {
            uint eid = AttachedEntity.GetValue(EntityIdComponent.EntityIdProperty);
            short x = (short)((e.NewValue.X - e.OldValue.X) * 32 * 128);
            short y = (short)((e.NewValue.Y - e.OldValue.Y) * 32 * 128);
            short z = (short)((e.NewValue.Z - e.OldValue.Z) * 32 * 128);
            bool isOnGround = AttachedEntity.GetValue(EntityOnGroundComponent.IsOnGroundProperty);

            AttachedEntity.QueueOperation(() => AttachedEntity.GetComponent<ChunkEventBroadcastComponent>().GetGenerator().EntityRelativeMove(eid, x, y, z, isOnGround));
        }
    }
}

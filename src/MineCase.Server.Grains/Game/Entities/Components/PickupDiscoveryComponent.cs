using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MineCase.Engine;
using MineCase.Graphics;
using MineCase.Server.Components;
using MineCase.Server.Game.BlockEntities;
using MineCase.Server.Network;
using MineCase.Server.Network.Play;

namespace MineCase.Server.Game.Entities.Components
{
    internal class PickupDiscoveryComponent : EntityDiscoveryComponentBase<PickupGrain>, IHandle<DestroyEntity>, IHandle<SpawnEntity>
    {
        public PickupDiscoveryComponent(string name = "pickupDiscovery")
            : base(name)
        {
        }

        protected override Task SendSpawnPacket(ClientPlayPacketFactory generator)
        {
            // for items, the int value is ignored, but should be set to 1 to indicate that velocity is present.
            return generator.SpawnObject(AttachedEntity.EntityId, AttachedEntity.UUID, 2, AttachedEntity.Position, AttachedEntity.Pitch, AttachedEntity.Yaw, 1);
        }

        Task IHandle<DestroyEntity>.Handle(DestroyEntity message)
        {
            return AttachedEntity.GetComponent<ChunkEventBroadcastComponent>().GetGenerator()
                .DestroyEntities(new[] { AttachedEntity.EntityId });
        }

        Task IHandle<SpawnEntity>.Handle(SpawnEntity message)
        {
            var pos = message.Position;
            var bb = BoundingBox.Item();
            var box = new Cuboid(new Point3d(pos.X, pos.Z, pos.Y), new Size(bb.X, bb.Y, bb.Z));
            AttachedEntity.SetLocalValue(ColliderComponent.ColliderShapeProperty, box);
            CompleteSpawn();

            return Task.CompletedTask;
        }
    }
}

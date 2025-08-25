using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Components;
using MineCase.Server.Game.BlockEntities;
using MineCase.Server.Network.Play;
using MineCase.Server.World;

namespace MineCase.Server.Game.Entities.Components
{
    internal class PlayerDiscoveryComponent : EntityDiscoveryComponentBase<PlayerGrain>, IHandle<PlayerLoggedIn>, IHandle<DestroyEntity>
    {
        public PlayerDiscoveryComponent(string name = "playerDiscovery")
            : base(name)
        {
        }

        protected override Task SendSpawnPacket(ClientPlayPacketGenerator generator)
        {
            var metadata = new EntityMetadata.Player
            {
                Health = AttachedEntity.GetValue(HealthComponent.HealthProperty)
            };

            return generator.SpawnPlayer(AttachedEntity.EntityId, AttachedEntity.UUID, AttachedEntity.Position, AttachedEntity.Pitch, AttachedEntity.HeadYaw, metadata);
        }

        Task IHandle<PlayerLoggedIn>.Handle(PlayerLoggedIn message)
        {
            CompleteSpawn();
            AttachedEntity.QueueOperation(() =>
            {
                return GrainFactory.GetGrain<IWorldPartition>(AttachedEntity.GetAddressByPartitionKey()).Enter(AttachedEntity);
            });
            return Task.CompletedTask;
        }

        Task IHandle<DestroyEntity>.Handle(DestroyEntity message)
        {
            if (AttachedEntity.EntityId != 0)
            {
                return AttachedEntity.GetComponent<ChunkEventBroadcastComponent>().GetGenerator()
                    .DestroyEntities(new[] { AttachedEntity.EntityId });
            }

            return Task.CompletedTask;
        }
    }
}

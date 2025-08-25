using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MineCase.Engine;
using MineCase.Server.Components;
using MineCase.Server.Game.BlockEntities;
using MineCase.Server.Network;
using MineCase.Server.Network.Play;

namespace MineCase.Server.Game.Entities.Components
{
    internal class MobDiscoveryComponent : EntityDiscoveryComponentBase<EntityGrain>, IHandle<SpawnMob>
    {
        public MobDiscoveryComponent(string name = "mobDiscovery")
            : base(name)
        {
        }

        Task IHandle<SpawnMob>.Handle(SpawnMob message)
        {
            AttachedEntity.Tell<SpawnEntity>(message);
            AttachedEntity.SetLocalValue(MobTypeComponent.MobTypeProperty, message.MobType);
            CompleteSpawn();

            // Logger.LogInformation($"Mob spawn, key: {AttachedEntity.GetAddressByPartitionKey()}");
            // Logger.LogInformation($"Mob spawn, type: {message.MobType}");
            return Task.CompletedTask;
        }

        protected override Task SendSpawnPacket(ClientPlayPacketGenerator generator)
        {
            MobType type = AttachedEntity.GetComponent<MobTypeComponent>().MobType;
            return generator.SpawnMob(AttachedEntity.EntityId, AttachedEntity.UUID, (byte)type, AttachedEntity.Position, AttachedEntity.Pitch, AttachedEntity.Yaw, new EntityMetadata.Entity { });
        }
    }
}

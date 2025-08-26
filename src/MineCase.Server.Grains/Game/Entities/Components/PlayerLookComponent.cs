// deprecated file
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
    internal class PlayerLookComponent : EntityLookComponentBase<EntityGrain>, IHandle<EntityLook>
    {
        public PlayerLookComponent(string name = "playerLook")
            : base(name)
        {
        }

        protected override Task SendLookPacket(ClientPlayPacketGenerator generator)
        {
            int eid = AttachedEntity.GetComponent<EntityIdComponent>().EntityId;
            float yaw = AttachedEntity.GetComponent<EntityLookComponent>().Yaw;
            float pitch = AttachedEntity.GetComponent<EntityLookComponent>().Pitch;
            bool onGround = AttachedEntity.GetComponent<EntityOnGroundComponent>().IsOnGround;

            // TODO player look
            // generator.;
            return Task.CompletedTask;
        }
    }
}

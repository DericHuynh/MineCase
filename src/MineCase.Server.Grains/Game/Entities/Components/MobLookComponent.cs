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
    internal class MobLookComponent : EntityLookComponentBase<EntityGrain>
    {
        public MobLookComponent(string name = "mobLook")
            : base(name)
        {
        }

        protected override Task SendLookPacket(ClientPlayPacketFactory generator)
        {
            int eid = AttachedEntity.GetComponent<EntityIdComponent>().EntityId;
            byte yaw = (byte)(AttachedEntity.GetComponent<EntityLookComponent>().Yaw / 360 * 255);
            byte pitch = (byte)(AttachedEntity.GetComponent<EntityLookComponent>().Pitch / 360 * 255);
            bool onGround = AttachedEntity.GetComponent<EntityOnGroundComponent>().IsOnGround;
            generator.EntityLook(eid, yaw, pitch, onGround);
            return Task.CompletedTask;
        }
    }
}

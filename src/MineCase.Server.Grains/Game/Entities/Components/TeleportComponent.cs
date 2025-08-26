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
    [Orleans.GenerateSerializer]
    internal class TeleportComponent : Component
    {
        [Orleans.Id(0)]
        private int _teleportId = 0;

        public TeleportComponent(string name = "teleport")
            : base(name)
        {
        }

        public int StartNew() => _teleportId++;

        public Task Teleport(EntityWorldPos position, float yaw, float pitch)
        {
            var generator = AttachedEntity.GetComponent<ClientboundPacketComponent>().GetGenerator();
            int teleportId = AttachedEntity.GetComponent<TeleportComponent>().StartNew();
            return generator.PositionAndLook(position.X, position.Y, position.Z, yaw, pitch, 0, teleportId);
        }

        public Task ConfirmTeleport(int teleportId)
        {
            return Task.CompletedTask;
        }
    }
}

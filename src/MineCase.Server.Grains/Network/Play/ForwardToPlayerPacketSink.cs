using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Protocol;
using MineCase.Server.Game.Entities;
using Orleans;
using Orleans.Concurrency;

namespace MineCase.Server.Network.Play
{
    [Orleans.GenerateSerializer]
    internal class ForwardToPlayerPacketSink : IPacketSink
    {
        [Id(0)]
        private readonly IPlayer _player;
        [Id(1)]
        private readonly IPacketPackager _packetPackager;

        public ForwardToPlayerPacketSink(IPlayer player, IPacketPackager packetPackager)
        {
            _player = player;
            _packetPackager = packetPackager;
        }

        public async Task SendPacket(IPacket packet)
        {
            var package = await _packetPackager.PreparePacket(packet);
            await SendPacket(package.PacketId, package.Data.AsImmutable());
        }

        public Task SendPacket(int packetId, Immutable<byte[]> data)
        {
            _player.Tell(new PacketForwardToPlayer
            {
                PacketId = packetId,
                Data = data.Value
            });
            return Task.CompletedTask;
        }
    }
}

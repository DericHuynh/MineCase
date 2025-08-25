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
    internal class KeepAliveComponent : Component, IHandle<BeginLogin>, IHandle<PlayerLoggedIn>, IHandle<KickPlayer>
    {
        [Orleans.Id(0)]
        private uint _keepAliveId = 0;
        [Orleans.Id(1)]
        public readonly Dictionary<long, DateTime> _keepAliveWaiters = new Dictionary<long, DateTime>();
        [Orleans.Id(2)]
        private bool _isOnline = false;

        private const int ClientKeepInterval = 6;

        [Orleans.Id(3)]
        public uint Ping { get; private set; }

        public KeepAliveComponent(string name = "keepAlive")
            : base(name)
        {
        }

        public Task ReceiveResponse(long keepAliveId)
        {
            if (_keepAliveWaiters.TryGetValue(keepAliveId, out var sendTime))
            {
                _keepAliveWaiters.Remove(keepAliveId);
                Ping = (uint)(DateTime.UtcNow - sendTime).TotalMilliseconds;
            }

            return Task.CompletedTask;
        }

        private async Task OnGameTick(object sender, GameTickArgs e)
        {
            if (_isOnline && _keepAliveWaiters.Count >= ClientKeepInterval)
            {
                _isOnline = false;
                await AttachedEntity.Tell(new KickPlayer());
            }
            else if (_isOnline && e.WorldAge % 20 == 0)
            {
                var id = _keepAliveId++;
                _keepAliveWaiters.Add(id, DateTime.UtcNow);
                await AttachedEntity.GetComponent<ClientboundPacketComponent>().GetGenerator().KeepAlive(id);
            }
        }

        Task IHandle<PlayerLoggedIn>.Handle(PlayerLoggedIn message)
        {
            _keepAliveWaiters.Clear();
            _isOnline = true;
            AttachedEntity.GetComponent<GameTickComponent>()
                .Tick += OnGameTick;
            return Task.CompletedTask;
        }

        Task IHandle<KickPlayer>.Handle(KickPlayer message)
        {
            AttachedEntity.GetComponent<GameTickComponent>()
                .Tick -= OnGameTick;
            _isOnline = false;
            return Task.CompletedTask;
        }

        Task IHandle<BeginLogin>.Handle(BeginLogin message)
        {
            AttachedEntity.GetComponent<GameTickComponent>()
                .Tick -= OnGameTick;
            _isOnline = false;
            return Task.CompletedTask;
        }
    }
}

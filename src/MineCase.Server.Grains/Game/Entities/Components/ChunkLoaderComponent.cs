using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Components;
using MineCase.Server.User;
using MineCase.World;
using Orleans;

namespace MineCase.Server.Game.Entities.Components
{
    [Orleans.GenerateSerializer]
    internal class ChunkLoaderComponent : Component<UserGrain>, IHandle<BeginLogin>, IHandle<PlayerLoggedIn>, IHandle<BindToUser>, IHandle<GameTick>
    {
        [Id(0)]
        private IUserChunkLoader _chunkLoader;
        [Id(1)]
        private bool _loaded;
        [Id(2)]
        private IPlayer _player;

        public ChunkLoaderComponent(string name = "chunkLoader")
            : base(name)
        {
        }

        protected override void OnAttached()
        {
            _loaded = false;
            _chunkLoader = GrainFactory.GetGrain<IUserChunkLoader>(AttachedEntity.GetPrimaryKey());
        }

        async Task IHandle<PlayerLoggedIn>.Handle(PlayerLoggedIn message)
        {
            _loaded = false;
            await _chunkLoader.JoinGame(await AttachedEntity.GetWorld(), _player);
            _loaded = true;
        }

        async Task IHandle<BindToUser>.Handle(BindToUser message)
        {
            _player = GrainFactory.GetGrain<IPlayer>(message.User.GetPrimaryKey());
            await _chunkLoader.SetClientPacketSink(await message.User.GetClientPacketSink());
        }

        Task IHandle<BeginLogin>.Handle(BeginLogin message)
        {
            _loaded = false;
            return Task.CompletedTask;
        }

        async Task IHandle<GameTick>.Handle(GameTick message)
        {
            if (_loaded)
                 await _chunkLoader.OnGameTick(message.Args, await _player.GetPosition());
        }
    }
}

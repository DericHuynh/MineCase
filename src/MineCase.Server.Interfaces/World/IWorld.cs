using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Server.Game;
using MineCase.World;
using MineCase.World.Generation;
using Orleans;
using Orleans.Concurrency;

namespace MineCase.Server.World
{
    public interface IWorld : IGrainWithStringKey
    {
        Task<uint> NewEntityId();

        [ReadOnly]
        Task<WorldTime> GetTime();

        [ReadOnly]
        Task<long> GetAge();

        Task OnGameTick(GameTickArgs e);

        [ReadOnly]
        Task<int> GetSeed();

        [ReadOnly]
        Task<GeneratorSettings> GetGeneratorSettings();

        Task<EntityWorldPos> GetSpawnPosition();

        [ReadOnly]
        Task<bool> HasSkyLight();
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Server.Game.Entities;
using MineCase.World;
using Orleans.Concurrency;

namespace MineCase.Server.World
{
    /// <summary>
    /// Why is this seperate from a chunk when the key is world, chunk_pos.
    /// Should merge with Chunk (A chunk is a world partition by definition).
    /// Although this is more lightweight than a chunk grain so for Activation Repartitioning this might work better idk.
    /// </summary>
    public interface IWorldPartition : IAddressByPartition
    {
        Task Enter(IPlayer player);

        Task Leave(IPlayer player);

        Task SubscribeDiscovery(IMineCaseEntity entity);

        Task UnsubscribeDiscovery(IMineCaseEntity entity);

        Task SubscribeTickEmitter(ITickEmitter tickEmitter);

        Task UnsubscribeTickEmitter(ITickEmitter tickEmitter);
    }
}

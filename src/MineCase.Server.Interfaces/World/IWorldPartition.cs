using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Server.Game.Entities;
using MineCase.World;
using Orleans.Concurrency;

namespace MineCase.Server.World
{
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

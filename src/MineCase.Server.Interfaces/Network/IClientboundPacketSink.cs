using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Protocol;
using MineCase.Protocol.Handshaking;
using Orleans;
using Orleans.Concurrency;

namespace MineCase.Server.Network
{
    public interface IClientboundPacketSink : IPacketSink, IGrainWithGuidKey
    {
        Task Close();

        Task Subscribe(IClientboundPacketObserver observer);

        Task UnSubscribe(IClientboundPacketObserver observer);
    }
}

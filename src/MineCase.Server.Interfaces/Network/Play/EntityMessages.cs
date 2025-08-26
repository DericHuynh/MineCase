using System;
using System.Collections.Generic;
using System.Text;
using MineCase.Engine;
using MineCase.Protocol;
using Orleans;
using Orleans.Concurrency;

namespace MineCase.Server.Network.Play
{
    [Orleans.GenerateSerializer]
    [Immutable]
    public sealed class PacketForwardToPlayer : IEntityMessage
    {
        [Id(0)]
        public int PacketId { get; set; }

        [Id(1)]
        public byte[] Data { get; set; }
    }

    [Orleans.GenerateSerializer]
    [Immutable]
    public sealed class PacketBroadcastToChunk : IEntityMessage
    {
        [Id(0)]
        public int PacketId { get; set; }

        [Id(1)]
        public byte[] Data { get; set; }
    }
}

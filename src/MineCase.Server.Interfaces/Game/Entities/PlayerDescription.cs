using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace MineCase.Server.Game.Entities
{
    [Orleans.GenerateSerializer]
    public sealed class PlayerDescription
    {
        [Orleans.Id(0)]
        public Guid UUID { get; set; }

        [Orleans.Id(1)]
        public string Name { get; set; }

        [Orleans.Id(2)]
        public GameMode GameMode { get; set; }

        [Orleans.Id(3)]
        public uint Ping { get; set; }

        [Orleans.Id(4)]
        public string DisplayName { get; set; }
    }
}

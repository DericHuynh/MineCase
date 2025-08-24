using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase.Server.World
{
    [Orleans.GenerateSerializer]
    public class WorldType
    {
        [Orleans.Id(0)]
        public int Id { get; set; }

        [Orleans.Id(1)]
        public string Name { get; set; }

        [Orleans.Id(2)]
        public int Version { get; set; }

        [Orleans.Id(3)]
        public bool CanBeCreated { get; set; }

        [Orleans.Id(4)]
        public bool Versioned { get; set; }

        [Orleans.Id(5)]
        public string NoticeInfo { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase
{
    [Orleans.GenerateSerializer]
    public struct GameMode
    {
        public enum Class : byte
        {
            Survival = 0,
            Creative = 1,
            Adventure = 2,
            Spectator = 3
        }

        [Orleans.Id(0)]
        public Class ModeClass { get; set; }

        [Orleans.Id(1)]
        public bool IsHardcore { get; set; }
    }
}

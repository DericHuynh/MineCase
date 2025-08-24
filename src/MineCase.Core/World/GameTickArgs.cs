using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase.World
{
    [Orleans.GenerateSerializer]
    public sealed class GameTickArgs
    {
        [Orleans.Id(0)]
        public TimeSpan DeltaTime { get; set; }

        [Orleans.Id(1)]
        public long WorldAge { get; set; }

        [Orleans.Id(2)]
        public long TimeOfDay { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase
{
    [Orleans.GenerateSerializer]
    public struct WorldTime
    {
        [Orleans.Id(0)]
        public long WorldAge { get; set; }

        [Orleans.Id(1)]
        public long TimeOfDay { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineCase.Server
{
    public static class ActivitySources
    {
        public static readonly ActivitySource NetworkActivitySource = new ActivitySource("MineCase.Network");
        public static readonly ActivitySource GameTickActivitySource = new ActivitySource("MineCase.GameTick");
    }
}

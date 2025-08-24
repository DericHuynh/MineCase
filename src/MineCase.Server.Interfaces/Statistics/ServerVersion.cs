using System;
using System.Collections.Generic;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Server.Statistics
{
    [Orleans.GenerateSerializer]
    public class ServerVersion
    {
        [Orleans.Id(0)]
        public string Name { get; set; }

        [Orleans.Id(1)]
        public uint Protocol { get; set; }
    }
}

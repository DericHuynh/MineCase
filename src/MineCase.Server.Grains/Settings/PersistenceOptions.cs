using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;

namespace MineCase.Server.Settings
{
    [Orleans.GenerateSerializer]
    public sealed class PersistenceOptions : IOptions<PersistenceOptions>
    {
        [Orleans.Id(0)]
        public string ConnectionString { get; set; }

        [Orleans.Id(1)]
        public string DatabaseName { get; set; }

        PersistenceOptions IOptions<PersistenceOptions>.Value => this;
    }
}

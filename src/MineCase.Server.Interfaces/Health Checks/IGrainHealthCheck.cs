using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace MineCase.Server.Health_Checks
{
    public interface IGrainHealthCheck
    {
        Task<GrainHealthStatus> CheckHealthAsync(CancellationToken cancellationToken);
    }
}

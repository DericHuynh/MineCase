using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orleans;
using Orleans.Concurrency;

namespace MineCase.Server.Health_Checks
{
    [StatelessWorker(1)]
    internal class BasicHealthCheckGrain : Grain, IBasicHealthCheckGrain
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new HealthCheckResult(HealthStatus.Healthy, description: "Basic Health Grain received."));
        }
    }
}

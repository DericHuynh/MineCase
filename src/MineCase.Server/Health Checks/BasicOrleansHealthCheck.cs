using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orleans;

namespace MineCase.Server.Health_Checks
{
    public class BasicOrleansHealthCheck : OrleansHealthCheckBase
    {
        public BasicOrleansHealthCheck(IClusterClient client)
            : base(client)
        {
        }

        protected override async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            try
            {
                return await _client.GetGrain<IBasicHealthCheckGrain>(Guid.Empty).CheckHealthAsync(context, cancellationToken);
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy($"Health check failed.", e);
            }
        }
    }
}

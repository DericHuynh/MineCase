using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MineCase.Server.Health_Checks;
using MineCase.Server.Settings;
using Orleans;

namespace MineCase.Gateway.Health_Checks
{
    public class SettingsHealthCheck : OrleansHealthCheckBase
    {
        public SettingsHealthCheck(IClusterClient client)
            : base(client)
        {
        }

        public override async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            try
            {
                GrainHealthStatus task_status =  await _client.GetGrain<IServerSettings>(0)
                                                              .CheckHealthAsync(cancellationToken);
                HealthCheckResult healthCheckResult = task_status.ToHealthCheckResult();
                return healthCheckResult;
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy($"Health check failed.", e);
            }
        }
    }
}

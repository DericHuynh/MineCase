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
    public abstract class OrleansHealthCheckBase : IHealthCheck
    {
        protected readonly IClusterClient _client;

        protected OrleansHealthCheckBase(IClusterClient client)
        {
            _client = client;
        }

        Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            return CheckHealthAsync(context, cancellationToken);
        }

        /// <summary>
        /// Perform the actual health check work within this implemented method.
        /// </summary>
        /// <param name="context">The health check context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><see cref="Task"/> of <see cref="HealthCheckResult"/>.</returns>
        protected abstract Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken);
    }
}

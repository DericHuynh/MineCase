using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orleans;
using Orleans.MultiClient;

namespace MineCase.Gateway.Health_Checks
{
    public abstract class OrleansHealthCheckBase : IHealthCheck 
    {
        protected readonly IOrleansClient _client;

        protected OrleansHealthCheckBase(IOrleansClient client) 
        {
              _client = client;
        }

        /// <summary>
        /// Entry into health check, ensures the client is initialized, if it is not returns a healthy status.
        /// </summary>
        /// <param name="context">The health check context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><see cref="Task"/> of <see cref="HealthCheckResult"/>.</returns>
        public virtual async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await CheckHealthGrainAsync(context, cancellationToken);
        }

        /// <summary>
        /// Perform the actual health check work within this implemented method.
        /// </summary>
        /// <param name="context">The health check context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><see cref="Task"/> of <see cref="HealthCheckResult"/>.</returns>
        protected abstract Task<HealthCheckResult> CheckHealthGrainAsync(HealthCheckContext context, CancellationToken cancellationToken);
    }
}

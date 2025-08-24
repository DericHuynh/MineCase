using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MineCase.Server.Settings;
using Orleans;
using Orleans.Concurrency;

namespace MineCase.Server.Health_Checks
{
    internal class BasicHealthCheckGrain : Grain, IBasicHealthCheckGrain
    {
        /// <summary>
        /// This doesnt work because orleans copier doesnt recognize HealthCheckResult.
        /// TODO: https://learn.microsoft.com/en-us/dotnet/orleans/host/configuration-guide/serialization?pivots=orleans-7-0#grain-storage-serializers.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>HealthCheckResult healthy or unhealthy.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            ServerSettings settings = await GrainFactory.GetGrain<IServerSettings>(0).GetSettings();

            if (settings != null)
                return new HealthCheckResult(HealthStatus.Healthy, description: "Basic Health Grain received.");
            return new HealthCheckResult(HealthStatus.Unhealthy, description: "Basic Health Grain couldnt get server settings.");
        }
    }
}

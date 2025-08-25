using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orleans;

#nullable enable

namespace MineCase.Server.Health_Checks
{
    [GenerateSerializer]
    public class GrainHealthStatus
    {
        [Id(0)]
        public HealthStatus Status { get; set; }

        [Id(1)]
        public string? Message { get; set; }

        [Id(2)]
        public Exception? Exception { get; set; }

        [Id(3)]
        public Dictionary<string, object>? Data { get; set; } // For additional details

        public static GrainHealthStatus Healthy(string message = "Grain is healthy.", Dictionary<string, object>? data = null)
        {
            return new GrainHealthStatus { Status = HealthStatus.Healthy, Message = message, Data = data };
        }

        public static GrainHealthStatus Unhealthy(string message = "Grain is unhealthy.", Exception? exception = null, Dictionary<string, object>? data = null)
        {
            return new GrainHealthStatus { Status = HealthStatus.Unhealthy, Message = message, Exception = exception, Data = data };
        }

        public static GrainHealthStatus FromHealthCheckResult(HealthCheckResult result)
        {
            var healthStatus = new GrainHealthStatus
            {
                Status = result.Status,
                Message = result.Description,
                Data = result.Data.ToDictionary()
            };

            // If the original result was unhealthy, and description is null, set a default message
            if (healthStatus.Status != HealthStatus.Healthy && string.IsNullOrWhiteSpace(healthStatus.Message))
            {
                healthStatus.Message = "Grain is unhealthy without specific description.";
            }

            healthStatus.Exception = result.Exception;

            return healthStatus;
        }

        public HealthCheckResult ToHealthCheckResult()
        {
            var healthCheckResult = new HealthCheckResult(
                status: this.Status,
                description: this.Message,
                exception: this.Exception,
                data: this.Data);

            return healthCheckResult;
        }
    }
}

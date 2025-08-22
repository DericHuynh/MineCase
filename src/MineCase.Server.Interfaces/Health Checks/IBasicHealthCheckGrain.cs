using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orleans;

namespace MineCase.Server.Health_Checks
{
    public interface IBasicHealthCheckGrain : IHealthCheck, IGrainWithGuidKey
    {
    }
}

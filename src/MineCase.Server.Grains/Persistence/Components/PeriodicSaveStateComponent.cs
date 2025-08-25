using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Components;
using MineCase.World;
using Orleans;

namespace MineCase.Server.Persistence.Components
{
    [Orleans.GenerateSerializer]
    internal class PeriodicSaveStateComponent : Component
    {
        [Id(0)]
        private readonly TimeSpan _periodTime;

        public PeriodicSaveStateComponent(TimeSpan periodTime, string name = "periodicSaveState")
            : base(name)
        {
            _periodTime = periodTime;
        }

        protected override void OnAttached()
        {
            AttachedEntity.RegisterGrainTimer(SaveIfDirty, _periodTime, _periodTime);
        }

        private Task SaveIfDirty()
        {
            if (AttachedEntity.ValueStorage.IsDirty)
            {
                return AttachedEntity.WriteStateAsync();
            }

            return Task.CompletedTask;
        }
    }
}

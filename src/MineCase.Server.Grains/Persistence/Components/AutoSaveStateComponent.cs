using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Components;
using MineCase.World;

namespace MineCase.Server.Persistence.Components
{
    [Orleans.GenerateSerializer]
    internal class AutoSaveStateComponent : Component
    {
        [Orleans.Id(0)]
        private readonly int _periodTime;

        public const int PerMinute = 20 * 60;

        public AutoSaveStateComponent(int periodTime, string name = "autoSaveState")
            : base(name)
        {
            _periodTime = periodTime;
        }

        protected override void OnAttached()
        {
            var tickComponent = AttachedEntity.GetComponent<GameTickComponent>();
            if (tickComponent != null)
                tickComponent.Tick += OnGameTick;
        }

        public Task OnGameTick(object sender, GameTickArgs e)
        {
            if (AttachedEntity.ValueStorage.IsDirty && (e.WorldAge % _periodTime == 0))
            {
                return AttachedEntity.WriteStateAsync();
            }

            return Task.CompletedTask;
        }
    }
}

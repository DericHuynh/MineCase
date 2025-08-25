using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Components;
using MineCase.Server.Network.Play;
using MineCase.World;

namespace MineCase.Server.Game.Entities.Components
{
    /// <summary>
    /// When player health is less than 0, respawn him.
    /// </summary>
    internal class DeathComponent : Component<PlayerGrain>
    {
        public static readonly DependencyProperty<bool> IsDeathProperty =
            DependencyProperty.Register<bool>("IsDeath", typeof(DeathComponent), new PropertyMetadata<bool>(false, OnDeath));

        public bool IsDeath => AttachedEntity.GetValue(IsDeathProperty);

        public DeathComponent(string name = "death")
            : base(name)
        {
        }

        protected override void OnAttached()
        {
        }

        protected override void OnDetached()
        {
        }

        private async Task Respawn()
        {
            var generator = AttachedEntity.GetComponent<ClientboundPacketComponent>().GetGenerator();

            var teleportComponent = AttachedEntity.GetComponent<TeleportComponent>();
            var world = AttachedEntity.GetWorld();
            var spawnPos = await world.GetSpawnPosition();
            await teleportComponent.Teleport(spawnPos, 0, 0);
            await generator.Respawn(Dimension.Overworld, await world.GetSeed(), new GameMode { ModeClass = GameMode.Class.Survival, IsHardcore = false }, LevelTypes.Default);
            AttachedEntity.SetLocalValue(HealthComponent.HealthProperty, AttachedEntity.GetValue(HealthComponent.MaxHealthProperty));
            AttachedEntity.SetLocalValue(FoodComponent.FoodProperty, AttachedEntity.GetValue(FoodComponent.MaxFoodProperty));
            AttachedEntity.SetLocalValue(DeathComponent.IsDeathProperty, false);
        }

        private void OnDeath(PropertyChangedEventArgs<bool> e)
        {
            Task.Run(Respawn);
        }

        private static void OnDeath(object sender, PropertyChangedEventArgs<bool> e)
        {
            var component = ((Entity)sender).GetComponent<DeathComponent>();
            component.OnDeath(e);
        }
    }
}

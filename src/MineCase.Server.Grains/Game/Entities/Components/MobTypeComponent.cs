using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;

namespace MineCase.Server.Game.Entities.Components
{
    internal class MobTypeComponent : Component
    {
        public static readonly DependencyProperty<MobType> MobTypeProperty =
            DependencyProperty.Register<MobType>("MobType", typeof(MobTypeComponent));

        public MobType MobType => AttachedEntity.GetValue(MobTypeProperty);

        public MobTypeComponent(string name = "mobType")
            : base(name)
        {
        }

        public void SetMobType(MobType value) =>
            AttachedEntity.SetLocalValue(MobTypeProperty, value);
    }
}

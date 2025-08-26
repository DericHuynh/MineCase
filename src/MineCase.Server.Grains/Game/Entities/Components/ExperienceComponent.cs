using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Network.Play;

namespace MineCase.Server.Game.Entities.Components
{
    [Orleans.GenerateSerializer]
    internal class ExperienceComponent : Component
    {
        public static readonly DependencyProperty<int> ExperienceProperty =
            DependencyProperty.Register<int>("Experience", typeof(ExperienceComponent));

        [Orleans.Id(0)]
        private int _levelMaxExp = 7;
        [Orleans.Id(1)]
        private int _totalExp = 0;
        [Orleans.Id(2)]
        private int _level = 0;

        public int Experience => AttachedEntity.GetValue(ExperienceProperty);

        public float ExperienceBar => (float)Experience / _levelMaxExp;

        public int Level => _level;

        public int TotalExperience => _totalExp;

        public ExperienceComponent(string name = "experience")
            : base(name)
        {
        }
    }
}

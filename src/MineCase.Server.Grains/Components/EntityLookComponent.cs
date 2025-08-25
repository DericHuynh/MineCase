using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Network.Play;

namespace MineCase.Server.Components
{
    internal class EntityLookComponent : Component
    {
        public static readonly DependencyProperty<float> PitchProperty =
            DependencyProperty.Register<float>("Pitch", typeof(EntityLookComponent));

        public static readonly DependencyProperty<float> YawProperty =
            DependencyProperty.Register<float>("Yaw", typeof(EntityLookComponent));

        public static readonly DependencyProperty<float> HeadYawProperty =
            DependencyProperty.Register<float>("HeadYaw", typeof(EntityLookComponent));

        public float Pitch => AttachedEntity.GetValue(PitchProperty);

        public float Yaw => AttachedEntity.GetValue(YawProperty);

        public float HeadYaw => AttachedEntity.GetValue(HeadYawProperty);

        public EntityLookComponent(string name = "entityLook")
            : base(name)
        {
        }

        public void SetPitch(float value) =>
            AttachedEntity.SetLocalValue(PitchProperty, value);

        public void SetYaw(float value) =>
            AttachedEntity.SetLocalValue(YawProperty, value);

        public void SetHeadYaw(float value) =>
            AttachedEntity.SetLocalValue(HeadYawProperty, value);
    }
}

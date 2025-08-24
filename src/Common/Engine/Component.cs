using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ServiceProviderType = System.IServiceProvider;

namespace MineCase.Engine
{
    internal interface IComponentIntern
    {
        void Attach(DependencyObject dependencyObject, ServiceProviderType serviceProvider);

        void Detach();

        int GetMessageOrder(object message);
    }

    [Orleans.GenerateSerializer]
    public abstract partial class Component : IComponentIntern
    {
        [Orleans.Id(0)]
        public string Name { get; }

        [Orleans.Id(1)]
        protected DependencyObject AttachedObject { get; private set; }

        [Orleans.Id(2)]
        protected ServiceProviderType ServiceProvider { get; private set; }

        public Component(string name)
        {
            Name = name;
        }

        void IComponentIntern.Attach(DependencyObject dependencyObject, ServiceProviderType serviceProvider)
        {
            AttachedObject = dependencyObject;
            ServiceProvider = serviceProvider;
            AttatchPartial(dependencyObject, serviceProvider);
            OnAttached();
        }

        partial void AttatchPartial(DependencyObject dependencyObject, ServiceProviderType serviceProvider);

        void IComponentIntern.Detach()
        {
            OnDetached();
            AttachedObject = null;
        }

        /// <summary>
        /// When a component is attached to an entity.
        /// </summary>
        protected virtual void OnAttached()
        {
        }

        /// <summary>
        /// When a component is detached from an entity.
        /// </summary>
        protected virtual void OnDetached()
        {
        }

        /// <summary>
        /// Get the message processing order.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <returns>Processing order (smaller numbers appear first).</returns>
        public virtual int GetMessageOrder(object message)
        {
            return 0;
        }
    }

    public abstract class Component<T> : Component
        where T : DependencyObject
    {
        public new T AttachedObject => (T)base.AttachedObject;

        public Component(string name)
            : base(name)
        {
        }
    }
}

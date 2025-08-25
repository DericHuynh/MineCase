using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using ServiceProviderType = System.IServiceProvider; // Alias for clarity

namespace MineCase.Engine
{
    /// <summary>
    /// Defines the internal contract for a component, outlining how it interacts with an <see cref="Entity"/>.
    /// This interface provides methods for attaching and detaching from an entity, and for determining
    /// the processing order of messages it handles.
    /// </summary>
    internal interface IComponent
    {
        /// <summary>
        /// Attaches this component to a specified <see cref="Entity"/>.
        /// This method is called by the entity when the component is added.
        /// </summary>
        /// <param name="dependencyObject">The entity to which this component is being attached.</param>
        /// <param name="serviceProvider">The service provider available from the entity's context.</param>
        void Attach(Entity dependencyObject, ServiceProviderType serviceProvider);

        /// <summary>
        /// Detaches this component from its current entity.
        /// This method is called by the entity when the component is removed.
        /// </summary>
        void Detach();

        /// <summary>
        /// Retrieves the processing order for a given message.
        /// Components with a smaller order number will process the message before those with larger numbers.
        /// </summary>
        /// <param name="message">The message for which to determine the processing order.</param>
        /// <returns>An integer representing the processing order, where lower values indicate higher priority.</returns>
        int GetMessageOrder(object message);
    }

    /// <summary>
    /// The base abstract class for all components in the Entity-Component-System.
    /// Components encapsulate specific functionalities and data that can be added to an <see cref="Entity"/>.
    /// They are serializable by Orleans for persistence and state management across grain activations.
    /// </summary>
    [Orleans.GenerateSerializer] // Instructs Orleans to generate a serializer for this class
    public abstract partial class Component : IComponent
    {
        /// <summary>
        /// Gets the unique name of this component.
        /// </summary>
        [Orleans.Id(0)] // Orleans field ID for serialization
        public string Name { get; }

        /// <summary>
        /// Gets the <see cref="Entity"/> to which this component is currently attached.
        /// This property is set during the <see cref="Attach"/> operation.
        /// </summary>
        [Orleans.Id(1)]
        protected Entity AttachedEntity { get; private set; }

        /// <summary>
        /// Gets the service provider, allowing access to services registered within the Orleans host.
        /// </summary>
        [Orleans.Id(2)]
        protected ServiceProviderType ServiceProvider { get; private set; }

        /// <summary>
        /// Gets the Orleans grain factory, used for obtaining references to other grains.
        /// </summary>
        [Orleans.Id(3)]
        protected IGrainFactory GrainFactory { get; private set; }

        /// <summary>
        /// Gets the logger instance for this component, allowing for structured logging.
        /// </summary>
        [Orleans.Id(4)]
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Component"/> class.
        /// </summary>
        /// <param name="name">The unique name of the component.</param>
        public Component(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Implements <see cref="IComponent.Attach"/>. This method sets up the component's references
        /// to the host entity, service provider, grain factory, and logger, then calls <see cref="OnAttached"/>.
        /// </summary>
        /// <param name="dependencyObject">The entity to which this component is being attached.</param>
        /// <param name="serviceProvider">The service provider from the entity.</param>
        void IComponent.Attach(Entity dependencyObject, ServiceProviderType serviceProvider)
        {
            AttachedEntity = dependencyObject;
            ServiceProvider = serviceProvider;
            GrainFactory = serviceProvider.GetService<IGrainFactory>();
            Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType()); // Get a logger specific to this component's type
            OnAttached();
        }

        /// <summary>
        /// Implements <see cref="IComponent.Detach"/>. This method cleans up the component's references
        /// and then calls <see cref="OnDetached"/>.
        /// </summary>
        void IComponent.Detach()
        {
            OnDetached();
            AttachedEntity = null; // Clear reference to prevent memory leaks
        }

        /// <summary>
        /// A hook method that is called immediately after the component has been successfully attached to an entity.
        /// Derived classes can override this method to perform initialization logic specific to their attachment.
        /// </summary>
        protected virtual void OnAttached()
        {
        }

        /// <summary>
        /// A hook method that is called immediately before the component is detached from an entity.
        /// Derived classes can override this method to perform cleanup logic specific to their detachment.
        /// </summary>
        protected virtual void OnDetached()
        {
        }

        /// <summary>
        /// Implements <see cref="IComponent.GetMessageOrder"/>.
        /// Retrieves the processing order for a given message. By default, all messages have an order of 0.
        /// Derived classes can override this method to implement custom message prioritization.
        /// </summary>
        /// <param name="message">The message for which to determine the processing order.</param>
        /// <returns>An integer representing the processing order. Lower values mean higher priority.</returns>
        public virtual int GetMessageOrder(object message)
        {
            return 0; // Default order
        }
    }

    /// <summary>
    /// A generic base class for components that are designed to be attached to a specific type of <see cref="Entity"/>.
    /// This provides a strongly-typed <see cref="AttachedEntity"/> property, simplifying access to entity-specific members.
    /// </summary>
    /// <typeparam name="T">The specific type of <see cref="Entity"/> this component expects to be attached to.</typeparam>
    public abstract class Component<T> : Component
        where T : Entity
    {
        /// <summary>
        /// Gets the strongly-typed <see cref="Entity"/> to which this component is currently attached.
        /// This property safely casts the base <see cref="Component.AttachedEntity"/> to type <typeparamref name="T"/>.
        /// </summary>
        public new T AttachedEntity => (T)base.AttachedEntity;

        /// <summary>
        /// Initializes a new instance of the <see cref="Component{T}"/> class.
        /// </summary>
        /// <param name="name">The unique name of the component.</param>
        public Component(string name)
            : base(name)
        {
        }
    }
}
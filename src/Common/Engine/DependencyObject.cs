using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MineCase.Engine.Data;
using MineCase.Engine.Serialization;
using MineCase.Library;
using Orleans;
using Orleans.Concurrency;
using Orleans.Streams;

namespace MineCase.Engine
{
    /// <summary>
    /// Represents an entity in an Entity-Component-System (ECS) architecture, extended with message routing capabilities.
    /// This abstract class acts as a central hub for managing components, their properties, and message handling.
    /// It also integrates with Orleans Grain features for distributed state management and activation lifecycle.
    /// </summary>
    public abstract partial class Entity : Grain, IEntity
    {
        // Static members for reflection and message handling optimization
        private static readonly MethodInfo _raisePropertyChangedHelper = typeof(Entity).GetRuntimeMethods().Single(o => o.Name == nameof(RaisePropertyChangedHelper));
        private static readonly ConcurrentDictionary<Type, Delegate> _messageCaller = new ConcurrentDictionary<Type, Delegate>();

        // Publicly exposed interface for dependency value storage
        public IDependencyValueStorage ValueStorage => _valueStorage;

        // Internal state for managing components and their properties
        private Dictionary<string, IComponent> _components;
        private Dictionary<IComponent, int> _indexes;
        private MultiValueDictionary<Type, IComponent> _messageHandlers;
        private DependencyValueStorage _valueStorage;

        // Type of the concrete entity instance for reflection purposes
        internal readonly Type _realType;

        // Handlers for property change events, both specific and general
        private readonly Dictionary<DependencyProperty, Delegate> _propertyChangedHandlers = new Dictionary<DependencyProperty, Delegate>();
        private readonly Dictionary<DependencyProperty, Delegate> _propertyChangedHandlersGen = new Dictionary<DependencyProperty, Delegate>();
        private Delegate _anyPropertyChangedHandler;

        // Counter for assigning unique indexes to components, used for message handling order
        private int _index = 0;

        // Orleans specific state members
        [Id(3)]
        private bool _isDestroyed = false;

        [Id(4)]
        private readonly Queue<Func<Task>> _operationQueue = new Queue<Func<Task>>();

        /// <summary>
        /// Gets the logger for this entity.
        /// Injected by the Orleans service provider.
        /// </summary>
        [Id(5)]
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// Sets up internal dictionaries and captures the concrete type of the entity.
        /// </summary>
        public Entity()
        {
            _realType = this.GetType();
            _components = new Dictionary<string, IComponent>();
            _indexes = new Dictionary<IComponent, int>();
            _messageHandlers = new MultiValueDictionary<Type, IComponent>();
        }

        /// <summary>
        /// Called when the grain is activated. Initializes the logger, pre-loads components,
        /// reads the entity's state, and then initializes other components.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the activation process.</param>
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Logger = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
            InitializePreLoadComponent();
            await ReadStateAsync();
            InitializeComponents();
        }

        /// <summary>
        /// Called when the grain is deactivated. Writes the entity's state before deactivation.
        /// </summary>
        /// <param name="reason">The reason for deactivation.</param>
        /// <param name="cancellationToken">Cancellation token for the deactivation process.</param>
        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            await WriteStateAsync();
            await base.OnDeactivateAsync(reason, cancellationToken);
        }

        /// <summary>
        /// Marks the entity for destruction. Upon deactivation, its state will be cleared.
        /// </summary>
        public void Destroy()
        {
            _isDestroyed = true;
            DeactivateOnIdle();
        }

        /// <summary>
        /// Hook for initializing components after the entity's state has been read.
        /// Concrete entity implementations should override this to set up their components.
        /// </summary>
        protected virtual void InitializeComponents()
        {
        }

        /// <summary>
        /// Hook for initializing components that need to be set up before the entity's state is read.
        /// Concrete entity implementations can override this.
        /// </summary>
        protected virtual void InitializePreLoadComponent()
        {
        }

        /// <summary>
        /// Reads the entity's persistent state, deserializing it into the <see cref="_valueStorage"/>.
        /// If no state exists, a new <see cref="DependencyValueStorage"/> is created.
        /// Subscribes to value change events and notifies components via <see cref="AfterReadState"/> message.
        /// </summary>
        public async Task ReadStateAsync()
        {
            var state = await DeserializeStateAsync();
            if (state == null || state.ValueStorage == null)
            {
                _valueStorage = new Data.DependencyValueStorage();
            }
            else
            {
                _valueStorage = (Data.DependencyValueStorage)state.ValueStorage;
            }

            _valueStorage.CurrentValueChanged += ValueStorage_CurrentValueChanged;
            await Tell(AfterReadState.Default);
        }

        /// <summary>
        /// Writes the entity's current state to persistence. If the entity is marked for destruction,
        /// its state is cleared instead. Notifies components via <see cref="BeforeWriteState"/> message.
        /// </summary>
        public async Task WriteStateAsync()
        {
            try
            {
                if (_isDestroyed)
                {
                    await ClearStateAsync();
                }
                else
                {
                    await Tell(BeforeWriteState.Default);
                    var state = new DependencyObjectState
                    {
                        GrainKeyString = GrainReference.GetGrainId().ToString(),
                        ValueStorage = _valueStorage
                    };

                    await SerializeStateAsync(state);
                    ValueStorage.IsDirty = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }
        }

        /// <summary>
        /// Abstract method for deserializing the entity's state.
        /// Concrete implementations must provide logic to load the state from storage.
        /// </summary>
        /// <returns>A task that returns the deserialized <see cref="DependencyObjectState"/>.</returns>
        protected virtual Task<DependencyObjectState> DeserializeStateAsync()
        {
            return Task.FromResult<DependencyObjectState>(null);
        }

        /// <summary>
        /// Abstract method for serializing the entity's state.
        /// Concrete implementations must provide logic to save the state to storage.
        /// </summary>
        /// <param name="state">The <see cref="DependencyObjectState"/> to serialize.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task SerializeStateAsync(DependencyObjectState state)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Abstract method for clearing the entity's persistent state.
        /// Concrete implementations must provide logic to delete the state from storage.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task ClearStateAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Enqueues an asynchronous operation to be executed in sequence.
        /// </summary>
        /// <param name="operation">The asynchronous operation to enqueue.</param>
        public void QueueOperation(Func<Task> operation)
        {
            _operationQueue.Enqueue(operation);
        }

        /// <summary>
        /// Executes all pending operations in the queue in the order they were enqueued.
        /// </summary>
        /// <returns>A task representing the asynchronous operation of clearing the queue.</returns>
        public async Task ClearOperationQueue()
        {
            while (_operationQueue.Count != 0)
            {
                await _operationQueue.Dequeue()();
            }
        }

        /// <summary>
        /// Retrieves an Orleans stream for publishing or subscribing to messages.
        /// </summary>
        /// <typeparam name="T">The type of the stream messages.</typeparam>
        /// <param name="providerName">The name of the stream provider.</param>
        /// <param name="streamId">The GUID of the stream.</param>
        /// <param name="streamNamespace">The namespace of the stream.</param>
        /// <returns>An <see cref="IAsyncStream{T}"/> instance.</returns>
        public IAsyncStream<T> GetStream<T>(string providerName, Guid streamId, string streamNamespace)
        {
            return this.GetStreamProvider(providerName).GetStream<T>(streamNamespace, streamId);
        }

        /// <summary>
        /// Returns the first component of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of the component to retrieve.</typeparam>
        /// <returns>The component instance if found, otherwise <see langword="null"/>.</returns>
        public T GetComponent<T>()
            where T : Component
        {
            foreach (var component in _components)
            {
                if (component.Value is T result)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Sets a component on this entity. If a component with the same name already exists,
        /// it is replaced. The new component is attached, and message handlers are subscribed.
        /// </summary>
        /// <param name="component">The component to set.</param>
        public void SetComponent(Component component)
        {
            var name = component.Name;
            if (_components.TryGetValue(name, out var old))
            {
                // If the old component is the same instance, do nothing.
                if (old == component) return;

                // Unsubscribe and detach the old component
                Unsubscribe(old);
                old.Detach();
                _indexes.Remove(old);
                _components.Remove(name);
            }

            // Add the new component
            _components.Add(name, component);
            _indexes.Add(component, _index++); // Assign a unique index for ordering
            ((IComponent)component).Attach(this, ServiceProvider); // Attach to this entity
            Subscribe(component); // Subscribe its message handlers
        }

        /// <summary>
        /// Removes all components of a specified type from this entity.
        /// </summary>
        /// <typeparam name="T">The type of components to clear.</typeparam>
        public void ClearComponent<T>()
            where T : Component
        {
            // Find all components matching the type
            var componentsToRemove = _components.Where(o => o.Value is T).ToList(); // .ToList() to avoid modifying collection while iterating

            foreach (var componentEntry in componentsToRemove)
            {
                var component = componentEntry.Value;
                Unsubscribe(component);
                component.Detach();
                _indexes.Remove(component);
                _components.Remove(componentEntry.Key);
            }
        }

        /// <summary>
        /// Gets the current effective value of a dependency property.
        /// This method checks the local value storage, non-default property values, and finally the default value.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="property">The dependency property.</param>
        /// <returns>The effective value of the property.</returns>
        public T GetValue<T>(DependencyProperty<T> property)
        {
            T value;

            // Try to get value from local storage, then non-default (e.g., inherited/default from metadata)
            if (!(_valueStorage.TryGetCurrentValue(property, out value) ||
                property.TryGetNonDefaultValue(this, _realType, out value)))
            {
                // If neither found, return the default value for the property or default(T)
                return GetDefaultValue(property);
            }

            return value;
        }

        /// <summary>
        /// Sets the current effective value of a dependency property.
        /// If the property is currently settable, its value is directly updated. Otherwise,
        /// it attempts to set a local value.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="property">The dependency property to set.</param>
        /// <param name="value">The new value for the property.</param>
        public void SetCurrentValue<T>(DependencyProperty<T> property, T value)
        {
            IEffectiveValue<T> eValue;

            // Try to get the effective value and check if it can be set
            if (_valueStorage.TryGetCurrentEffectiveValue(property, out eValue) && eValue.CanSetValue)
            {
                eValue.SetValue(value);
            }
            else
            {
                // Fallback to setting a local value if effective value not found or not settable
                this.SetLocalValue(property, value);
            }
        }

        /// <summary>
        /// Event handler for <see cref="DependencyValueStorage.CurrentValueChanged"/>.
        /// Invokes the generic <see cref="RaisePropertyChangedHelper{T}"/> method using reflection
        /// to handle property change notifications for specific property types.
        /// </summary>
        /// <param name="sender">The source of the event (the value storage).</param>
        /// <param name="e">The event arguments containing property and value change information.</param>
        private void ValueStorage_CurrentValueChanged(object sender, CurrentValueChangedEventArgs e)
        {
            _raisePropertyChangedHelper.MakeGenericMethod(e.Property.PropertyType).Invoke(this, new object[] { e.Property, e });
        }

        /// <summary>
        /// Registers a type-specific property changed handler for a <see cref="DependencyProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="property">The dependency property.</param>
        /// <param name="handler">The handler to register.</param>
        public void RegisterPropertyChangedHandler<T>(DependencyProperty<T> property, EventHandler<PropertyChangedEventArgs<T>> handler)
        {
            if (_propertyChangedHandlers.TryGetValue(property, out var existingHandler))
                _propertyChangedHandlers[property] = Delegate.Combine(existingHandler, handler);
            else
                _propertyChangedHandlers[property] = handler;
        }

        /// <summary>
        /// Removes a type-specific property changed handler for a <see cref="DependencyProperty{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="property">The dependency property.</param>
        /// <param name="handler">The handler to remove.</param>
        public void RemovePropertyChangedHandler<T>(DependencyProperty<T> property, EventHandler<PropertyChangedEventArgs<T>> handler)
        {
            if (_propertyChangedHandlers.TryGetValue(property, out var existingHandler))
            {
                var newHandler = Delegate.Remove(existingHandler, handler);
                if (newHandler == null)
                    _propertyChangedHandlers.Remove(property);
                else
                    _propertyChangedHandlers[property] = newHandler;
            }
        }

        /// <summary>
        /// Registers a general property changed handler for a <see cref="DependencyProperty"/>.
        /// This handler does not specify the property's value type.
        /// </summary>
        /// <param name="property">The dependency property.</param>
        /// <param name="handler">The handler to register.</param>
        public void RegisterPropertyChangedHandler(DependencyProperty property, EventHandler<PropertyChangedEventArgs> handler)
        {
            if (_propertyChangedHandlersGen.TryGetValue(property, out var existingHandler))
                _propertyChangedHandlersGen[property] = Delegate.Combine(existingHandler, handler);
            else
                _propertyChangedHandlersGen[property] = handler;
        }

        /// <summary>
        /// Removes a general property changed handler for a <see cref="DependencyProperty"/>.
        /// </summary>
        /// <param name="property">The dependency property.</param>
        /// <param name="handler">The handler to remove.</param>
        public void RemovePropertyChangedHandler(DependencyProperty property, EventHandler<PropertyChangedEventArgs> handler)
        {
            if (_propertyChangedHandlersGen.TryGetValue(property, out var existingHandler))
            {
                var newHandler = Delegate.Remove(existingHandler, handler);
                if (newHandler == null)
                    _propertyChangedHandlersGen.Remove(property);
                else
                    _propertyChangedHandlersGen[property] = newHandler;
            }
        }

        /// <summary>
        /// Registers a handler that is invoked for any property change on this entity.
        /// </summary>
        /// <param name="handler">The handler to register.</param>
        public void RegisterAnyPropertyChangedHandler(EventHandler<PropertyChangedEventArgs> handler)
        {
            _anyPropertyChangedHandler = Delegate.Combine(_anyPropertyChangedHandler, handler);
        }

        /// <summary>
        /// Removes a handler that was registered to be invoked for any property change.
        /// </summary>
        /// <param name="handler">The handler to remove.</param>
        public void RemoveAnyPropertyChangedHandler(EventHandler<PropertyChangedEventArgs> handler)
        {
            _anyPropertyChangedHandler = Delegate.Remove(_anyPropertyChangedHandler, handler);
        }

        /// <summary>
        /// Helper method to raise property changed events, handling type conversion and
        /// invoking registered handlers. This method is called via reflection from
        /// <see cref="ValueStorage_CurrentValueChanged"/>.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="property">The dependency property that changed.</param>
        /// <param name="e">The arguments containing the old and new values.</param>
        internal void RaisePropertyChangedHelper<T>(DependencyProperty<T> property, CurrentValueChangedEventArgs e)
        {
            // Determine old and new values, using default if not available in event args
            var oldValue = e.HasOldValue ? (T)e.OldValue : GetDefaultValue(property);
            var newValue = e.HasNewValue ? (T)e.NewValue : GetDefaultValue(property);

            // Avoid raising event if values are equal and both existed
            if (e.HasOldValue && e.HasNewValue && EqualityComparer<T>.Default.Equals(oldValue, newValue))
                return;

            var args = new PropertyChangedEventArgs<T>(property, oldValue, newValue);

            // Invoke handlers in a specific order:
            // 1. Property-specific static handlers
            property.RaisePropertyChanged(_realType, this, args);

            // 2. Local instance handlers (type-specific and general)
            InvokeLocalPropertyChangedHandlers(args);

            // 3. Virtual method for derived classes
            OnDependencyPropertyChanged(args);
        }

        /// <summary>
        /// Virtual method that can be overridden by derived entity classes to react to
        /// any dependency property change.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="args">The event arguments.</param>
        public virtual void OnDependencyPropertyChanged<T>(PropertyChangedEventArgs<T> args)
        {
        }

        /// <summary>
        /// Invokes local property changed handlers for a specific property change.
        /// This includes type-specific, general, and "any property changed" handlers.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="e">The property changed event arguments.</param>
        private void InvokeLocalPropertyChangedHandlers<T>(PropertyChangedEventArgs<T> e)
        {
            Delegate d;

            // Invoke type-specific handlers
            if (_propertyChangedHandlers.TryGetValue(e.Property, out d))
                ((EventHandler<PropertyChangedEventArgs<T>>)d).InvokeSerial(this, e);

            // Invoke general handlers
            if (_propertyChangedHandlersGen.TryGetValue(e.Property, out d))
                ((EventHandler<PropertyChangedEventArgs>)d).InvokeSerial(this, e);

            // Invoke "any property changed" handler
            ((EventHandler<PropertyChangedEventArgs>)_anyPropertyChangedHandler).InvokeSerial(this, e);
        }

        /// <summary>
        /// Retrieves the default value for a given dependency property.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="property">The dependency property.</param>
        /// <returns>The default value, or <see langword="default(T)"/> if no specific default is found.</returns>
        private T GetDefaultValue<T>(DependencyProperty<T> property)
        {
            T value;
            if (property.TryGetDefaultValue(this, _realType, out value))
                return value;
            return default(T);
        }

        /// <summary>
        /// Dynamically creates and caches a delegate to invoke the `Handle` method
        /// on an <see cref="IHandle{TMessage}"/> or <see cref="IHandle{TMessage, TResponse}"/>
        /// interface for a given message type. This uses expression trees for performance.
        /// </summary>
        /// <param name="messageType">The type of the message.</param>
        /// <returns>A delegate that can invoke the appropriate Handle method.</returns>
        private static Delegate GetOrAddMessageCaller(Type messageType)
        {
            return _messageCaller.GetOrAdd(messageType, k =>
            {
                // Find the IEntityMessage interface implemented by the message type
                var iface = (from i in messageType.GetInterfaces()
                             where i == typeof(IEntityMessage) ||
                             (i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IEntityMessage<>))
                             select i).Single();

                var paramExp = Expression.Parameter(typeof(IComponent), "c"); // Parameter for the component instance

                if (iface.IsConstructedGenericType)
                {
                    var responseType = iface.GetGenericArguments()[0]; // Get the response type
                    var messageParamExp = Expression.Parameter(typeof(IEntityMessage<>).MakeGenericType(responseType), "m"); // Parameter for the message
                    var handlerType = typeof(IHandle<,>).MakeGenericType(messageType, responseType); // Construct IHandle<TMessage, TResponse>
                    var handleMethod = handlerType.GetMethod("Handle"); // Get the Handle method info

                    // Create an expression: ((IHandle<TMessage, TResponse>)c).Handle((TMessage)m)
                    return Expression.Lambda(
                        Expression.Call(
                            Expression.Convert(paramExp, handlerType), // Cast component to handler type
                            handleMethod,
                            Expression.Convert(messageParamExp, messageType)), // Cast message to specific message type
                        paramExp,
                        messageParamExp).Compile(); // Compile into a delegate
                }
                else
                {
                    var messageParamExp = Expression.Parameter(typeof(IEntityMessage), "m"); // Parameter for the message
                    var handlerType = typeof(IHandle<>).MakeGenericType(messageType); // Construct IHandle<TMessage>
                    var handleMethod = handlerType.GetMethod("Handle"); // Get the Handle method info

                    // Create an expression: ((IHandle<TMessage>)c).Handle((TMessage)m)
                    return Expression.Lambda(
                        Expression.Call(
                            Expression.Convert(paramExp, handlerType), // Cast component to handler type
                            handleMethod,
                            Expression.Convert(messageParamExp, messageType)), // Cast message to specific message type
                        paramExp,
                        messageParamExp).Compile(); // Compile into a delegate
                }
            });
        }

        /// <summary>
        /// Retrieves all message types that a given component is configured to handle
        /// by inspecting its implemented <see cref="IHandle{TMessage}"/> or <see cref="IHandle{TMessage, TResponse}"/> interfaces.
        /// </summary>
        /// <param name="component">The component to inspect.</param>
        /// <returns>An enumerable of message types handled by the component.</returns>
        private IEnumerable<Type> GetComponentHandledMessageTypes(IComponent component)
        {
            foreach (var iface in component.GetType().GetInterfaces())
            {
                if (iface.IsConstructedGenericType)
                {
                    var genface = iface.GetGenericTypeDefinition();

                    // Check if the generic interface is IHandle<> or IHandle<,>
                    if (genface == typeof(IHandle<>) || genface == typeof(IHandle<,>))
                    {
                        // The first generic argument is always the message type
                        yield return iface.GetGenericArguments()[0];
                    }
                }
            }
        }

        /// <summary>
        /// Subscribes a component's message handlers by adding it to the
        /// internal <see cref="_messageHandlers"/> dictionary for each message type it handles.
        /// </summary>
        /// <param name="component">The component to subscribe.</param>
        private void Subscribe(IComponent component)
        {
            foreach (var type in GetComponentHandledMessageTypes(component))
                _messageHandlers.Add(type, component);
        }

        /// <summary>
        /// Unsubscribes a component's message handlers by removing it from the
        /// internal <see cref="_messageHandlers"/> dictionary for each message type it handles.
        /// </summary>
        /// <param name="component">The component to unsubscribe.</param>
        private void Unsubscribe(IComponent component)
        {
            foreach (var type in GetComponentHandledMessageTypes(component))
                _messageHandlers.Remove(type, component);
        }

        /// <inheritdoc />
        public Task Tell(IEntityMessage message)
        {
            return Tell(message, message.GetType());
        }

        /// <summary>
        /// Sends a message to all components registered to handle its specific type.
        /// This is a "fire-and-forget" operation, suitable for commands or notifications.
        /// </summary>
        /// <typeparam name="T">The concrete type of the message, must implement <see cref="IEntityMessage"/>.</typeparam>
        /// <param name="message">The message instance to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Tell<T>(T message)
            where T : IEntityMessage
        {
            return Tell(message, typeof(T));
        }

        /// <summary>
        /// Core message routing logic for "Tell" operations. It retrieves the appropriate
        /// message invoker and dispatches the message to all relevant component handlers
        /// in a specific order (based on message order and component index).
        /// This method is marked <see cref="OneWay"/> for Orleans, implying no direct response
        /// is expected by the caller.
        /// </summary>
        /// <param name="message">The message instance to send.</param>
        /// <param name="messageType">The specific type of the message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task Tell(IEntityMessage message, Type messageType)
        {
            // Get or create the dynamic invoker delegate for this message type
            var invoker = (Func<IComponent, IEntityMessage, Task>)GetOrAddMessageCaller(messageType);

            // Check if there are any registered handlers for this message type
            if (_messageHandlers.TryGetValue(messageType, out var handlers))
            {
                // Iterate through handlers, ordered by their message-specific order and then component index
                foreach (var handler in from h in handlers
                                        orderby h.GetMessageOrder(message), _indexes[h]
                                        select h)
                {
                    // Invoke the handler with the message
                    await invoker(handler, message);
                }
            }

            // Clear any queued operations after all message handlers have processed
            await ClearOperationQueue();
        }

        /// <inheritdoc />
        public async Task<TResponse> Ask<TResponse>(IEntityMessage<TResponse> message)
        {
            var response = await TryAsk(message);
            if (!response.Succeeded)
                throw new ReceiverNotFoundException(); // If no component successfully handled the message
            return response.Response;
        }

        /// <inheritdoc />
        public async Task<AskResult<TResponse>> TryAsk<TResponse>(IEntityMessage<TResponse> message)
        {
            var messageType = message.GetType();
            var invoker = (Func<IComponent, IEntityMessage<TResponse>, Task<TResponse>>)GetOrAddMessageCaller(messageType);

            // Check if there are any registered handlers for this message type
            if (_messageHandlers.TryGetValue(messageType, out var handlers))
            {
                // Iterate through handlers, ordered by their message-specific order and then component index
                foreach (var handler in from h in handlers
                                        orderby h.GetMessageOrder(message), _indexes[h]
                                        select h)
                {
                    // Invoke the handler and await its response
                    var response = await invoker(handler, message);

                    // Clear any queued operations after the first successful response
                    await ClearOperationQueue();
                    return new AskResult<TResponse> { Succeeded = true, Response = response };
                }
            }

            // If no handler was found or none returned a successful response (only the first one is used)
            return AskResult<TResponse>.Failed;
        }
    }
}
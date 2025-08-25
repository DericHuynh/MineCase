using System.Threading.Tasks;

namespace MineCase.Engine
{
    /// <summary>
    /// Represents a component's capability to handle a specific type of entity message without returning a response.
    /// Components implementing this interface can react to "fire-and-forget" messages sent via the <see cref="IEntity.Tell(IEntityMessage)"/> method.
    /// This pattern is common for commands, notifications, or events that don't require an immediate reply.
    /// </summary>
    /// <typeparam name="TMessage">The type of message this handler can process. Must implement <see cref="IEntityMessage"/>.</typeparam>
    public interface IHandle<TMessage>
        where TMessage : IEntityMessage
    {
        /// <summary>
        /// Processes the incoming message.
        /// This method is expected to perform actions based on the message content,
        /// but it does not return a value to the sender.
        /// </summary>
        /// <param name="message">The message instance to be handled.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation of handling the message.</returns>
        Task Handle(TMessage message);
    }

    /// <summary>
    /// Represents a component's capability to handle a specific type of entity message and return a response.
    /// Components implementing this interface can react to "ask" messages sent via the <see cref="IEntity.Ask{TResponse}(IEntityMessage{TResponse})"/>
    /// or <see cref="IEntity.TryAsk{TResponse}(IEntityMessage{TResponse})"/> methods.
    /// This pattern is used for queries or requests that require a specific data response.
    /// </summary>
    /// <typeparam name="TMessage">The type of message this handler can process. Must implement <see cref="IEntityMessage{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">The type of response that this handler will return for the given message.</typeparam>
    public interface IHandle<TMessage, TResponse>
        where TMessage : IEntityMessage<TResponse>
    {
        /// <summary>
        /// Processes the incoming message and generates a response.
        /// This method is expected to perform actions or retrieve data based on the message content
        /// and return a value of type <typeparamref name="TResponse"/>.
        /// </summary>
        /// <param name="message">The message instance to be handled.</param>
        /// <returns>A <see cref="Task{TResponse}"/> representing the asynchronous operation of handling the message and producing a response.</returns>
        Task<TResponse> Handle(TMessage message);
    }
}
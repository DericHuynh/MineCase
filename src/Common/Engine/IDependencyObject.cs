using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
#if ECS_SERVER
using Orleans;
using Orleans.Concurrency;
#endif

namespace MineCase.Engine
{
    [Immutable]
    public sealed class AskResult<TResponse>
    {
        public static readonly AskResult<TResponse> Failed = new AskResult<TResponse> { Succeeded = false };

        public bool Succeeded;

        public TResponse Response;
    }

    /// <summary>
    /// Represents an entity that can send and receive messages within the Entity-Component-System (ECS) architecture.
    /// This interface defines the core communication patterns for an entity, allowing it to:
    /// <list type="bullet">
    ///     <item>Send "fire-and-forget" messages (<see cref="Tell(IEntityMessage)"/>).</item>
    ///     <item>Send messages expecting a response (<see cref="Ask{TResponse}(IEntityMessage{TResponse})"/>).</item>
    ///     <item>Send messages expecting a response, with a mechanism to check for success (<see cref="TryAsk{TResponse}(IEntityMessage{TResponse})"/>).</item>
    /// </list>
    /// As an <see cref="IGrain"/>, entities are Orleans grains, implying they are distributed, stateful, and actors in a concurrent system.
    /// </summary>
    public interface IEntity : IGrain
    {
        /// <summary>
        /// Sends a message to the entity in a "fire-and-forget" manner.
        /// This method is marked with <see cref="OneWayAttribute"/>, indicating that the caller
        /// does not wait for the operation to complete or for a return value.
        /// The message is delivered to all internal components that are registered to handle its type.
        /// </summary>
        /// <param name="message">The message to send, which must implement <see cref="IEntityMessage"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. Due to <see cref="OneWayAttribute"/>, this task typically completes immediately from the caller's perspective.</returns>
        [OneWay]
        Task Tell(IEntityMessage message);

        /// <summary>
        /// Sends a message to the entity and asynchronously waits for a response of a specific type.
        /// The message is routed to internal components that are registered to handle it.
        /// If no component successfully handles the message and provides a response, a <see cref="ReceiverNotFoundException"/> is thrown.
        /// </summary>
        /// <typeparam name="TResponse">The expected type of the response.</typeparam>
        /// <param name="message">The message to send, which must implement <see cref="IEntityMessage{TResponse}"/>.</param>
        /// <returns>A <see cref="Task{TResponse}"/> representing the asynchronous operation, yielding the response from the handling component.</returns>
        /// <exception cref="ReceiverNotFoundException">Thrown if no component successfully processes the message and returns a response.</exception>
        Task<TResponse> Ask<TResponse>(IEntityMessage<TResponse> message);

        /// <summary>
        /// Attempts to send a message to the entity and asynchronously waits for a response.
        /// This method provides a more robust way to handle responses, as it returns an <see cref="AskResult{TResponse}"/>
        /// which explicitly indicates whether the operation succeeded and contains the response if it did.
        /// This avoids throwing an exception if no receiver is found or no response is generated.
        /// </summary>
        /// <typeparam name="TResponse">The expected type of the response.</typeparam>
        /// <param name="message">The message to send, which must implement <see cref="IEntityMessage{TResponse}"/>.</param>
        /// <returns>A <see cref="Task{AskResult}"/> representing the asynchronous operation, yielding an <see cref="AskResult{TResponse}"/>
        /// that indicates success/failure and contains the response if successful.</returns>
        Task<AskResult<TResponse>> TryAsk<TResponse>(IEntityMessage<TResponse> message);
    }
}

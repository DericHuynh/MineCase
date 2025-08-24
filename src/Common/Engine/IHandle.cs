using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MineCase.Engine
{
    /// <summary>
    /// Entity message processing interface.
    /// </summary>
    /// <typeparam name="TMessage">Message type.</typeparam>
    public interface IHandle<TMessage>
        where TMessage : IEntityMessage
    {
        /// <summary>
        /// Process message.
        /// </summary>
        /// <param name="message">Message.</param>
        Task Handle(TMessage message);
    }

    /// <summary>
    /// Entity message processing interface with response.
    /// </summary>
    /// <typeparam name="TMessage">Message type.</typeparam>
    /// <typeparam name="TResponse">Return type.</typeparam>
    public interface IHandle<TMessage, TResponse>
        where TMessage : IEntityMessage<TResponse>
    {
        /// <summary>
        /// Process message and get a response.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <returns>Reply.</returns>
        Task<TResponse> Handle(TMessage message);
    }
}

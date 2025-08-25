using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase.Engine
{
    /// <summary>
    /// Marker interface for messages that can be sent to an <see cref="IEntity"/> without expecting a direct response.
    /// Components implementing <see cref="IHandle{TMessage}"/> can process these messages.
    /// </summary>
    public interface IEntityMessage
    {
    }

    /// <summary>
    /// Marker interface for messages that can be sent to an <see cref="IEntity"/> while expecting a specific response.
    /// Components implementing <see cref="IHandle{TMessage, TResponse}"/> can process these messages and provide the <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected when this message is handled.</typeparam>
    public interface IEntityMessage<TResponse>
    {
    }

    /// <summary>
    /// Exception thrown when an <see cref="IEntity.Ask{TResponse}(IEntityMessage{TResponse})"/> operation fails
    /// because no component was found to handle the message or no response was successfully generated.
    /// </summary>
    public class ReceiverNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiverNotFoundException"/> class.
        /// </summary>
        public ReceiverNotFoundException()
            : base("No suitable receiver was found or no response was generated for the asked message.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiverNotFoundException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ReceiverNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiverNotFoundException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public ReceiverNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
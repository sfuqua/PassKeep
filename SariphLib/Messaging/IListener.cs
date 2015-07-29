using System.Threading.Tasks;

namespace SariphLib.Messaging
{
    /// <summary>
    /// An object capable of handling messages from a <see cref="MessageBus"/>.
    /// </summary>
    public interface IListener
    {
        /// <summary>
        /// Asynchronously deals with the specified message.
        /// </summary>
        /// <param name="message">The type of message.</param>
        /// <returns>A task representing the work to be done on the message.</returns>
        Task HandleMessage(IMessage message);
    }
}

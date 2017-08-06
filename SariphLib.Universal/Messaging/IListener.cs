// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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

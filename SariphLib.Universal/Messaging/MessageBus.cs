using System;
using System.Collections.Generic;

namespace SariphLib.Messaging
{
    /// <summary>
    /// Used to implement a pub-sub pattern between nested Views.
    /// </summary>
    public sealed class MessageBus
    {
        private Dictionary<string, ICollection<IListener>> subscriptions;

        /// <summary>
        /// Constructs the bus.
        /// </summary>
        public MessageBus()
        {
            this.subscriptions = new Dictionary<string, ICollection<IListener>>();
        }

        /// <summary>
        /// Subscribes <paramref name="listener"/> to messages named <paramref name="messageName"/>.
        /// </summary>
        /// <param name="messageName">The message being subscribed to.</param>
        /// <param name="listener">The object subscribing.</param>
        public void Subscribe(string messageName, IListener listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (!this.subscriptions.ContainsKey(messageName))
            {
                this.subscriptions[messageName] = new HashSet<IListener>();
            }

            this.subscriptions[messageName].Add(listener);
        }

        /// <summary>
        /// Unsubscribes <paramref name="listener"/> from messages <paramref name="messageName"/>.
        /// </summary>
        /// <typeparam name="T">The type of message.</typeparam>
        /// <param name="messageName">The message being unsubscribed from.</param>
        /// <param name="listener">The object unsubscribing.</param>
        public void Unsubscribe(string messageName, IListener listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (!this.subscriptions.ContainsKey(messageName))
            {
                throw new ArrayTypeMismatchException($"No subscriptions for message name {messageName} being tracked.");
            }

            this.subscriptions[messageName].Remove(listener);
        }

        /// <summary>
        /// Publishes a message to the bus.
        /// </summary>
        /// <param name="message">The message being published.</param>
        public void Publish(IMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (this.subscriptions.ContainsKey(message.Name))
            {
                ICollection<IListener> listeners = this.subscriptions[message.Name];
                foreach (IListener listener in listeners)
                {
                    listener.HandleMessage(message);
                }
            }
        }
    }
}

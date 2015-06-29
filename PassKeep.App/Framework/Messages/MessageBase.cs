using SariphLib.Infrastructure;
using SariphLib.Messaging;
using System;
using System.Reflection;

namespace PassKeep.Framework.Messages
{
    /// <summary>
    /// Handles naming convention for messages.
    /// </summary>
    public abstract class MessageBase : IMessage
    {
        private readonly string cachedName;

        /// <summary>
        /// Initializes the name variable.
        /// </summary>
        protected MessageBase()
        {
            this.cachedName = MessageBase.GetName(this.GetType());
        }

        /// <summary>
        /// Provides access to the message name.
        /// </summary>
        public string Name
        {
            get
            {
                return this.cachedName;
            }
        }

        /// <summary>
        /// Returns the message name for a corresponding type.
        /// </summary>
        /// <param name="t">The type of message.</param>
        /// <returns>The name of the message.</returns>
        public static string GetName(Type t)
        {
            Dbg.Assert(typeof(MessageBase).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()));
            return t.Name;
        }

        /// <summary>
        /// Returns the message name of the specified message type.
        /// </summary>
        /// <typeparam name="T">The type of message.</typeparam>
        /// <returns>The name of the message.</returns>
        public static string GetName<T>()
            where T : MessageBase
        {
            return GetName(typeof(T));
        }
    }
}

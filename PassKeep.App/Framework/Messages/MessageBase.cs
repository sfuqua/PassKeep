// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Diagnostics;
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
            this.cachedName = MessageBase.GetName(GetType());
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
            DebugHelper.Assert(typeof(MessageBase).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()));
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

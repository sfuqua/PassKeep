// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

namespace PassKeep.Framework.Messages
{
    /// <summary>
    /// A message for communicating that a database should go home.
    /// </summary>
    public sealed class NavHomeMessage : MessageBase
    {
        /// <summary>
        /// Constructs the message.
        /// </summary>
        public NavHomeMessage() { }
    }
}

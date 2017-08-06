// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

namespace SariphLib.Messaging
{
    /// <summary>
    /// A publishable message.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// The name of this message.
        /// </summary>
        string Name { get; }
    }
}

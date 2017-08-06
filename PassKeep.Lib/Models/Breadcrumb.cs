// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Models;

namespace PassKeep.Models
{
    /// <summary>
    /// Useless wrapper class for IKeePassGroup that tracks whether the Group is the first
    /// in a list of Groups, for display logic purposes.
    /// </summary>
    public class Breadcrumb
    {
        /// <summary>
        /// Wraps a Group with a new Breadcrumb.
        /// </summary>
        /// <param name="group">The Group to wrap.</param>
        /// <param name="isFirst">Whether this is the first Breadcrumb in a list, lol.</param>
        public Breadcrumb(IKeePassGroup group, bool isFirst = false)
        {
            Group = group;
            IsFirst = isFirst;
        }

        /// <summary>
        /// The Group represented by this breadcrumb.
        /// </summary>
        public IKeePassGroup Group
        {
            get;
            private set;
        }

        /// <summary>
        /// Stupid useless property for View logic because WinRT XAML is garbage.
        /// </summary>
        public bool IsFirst
        {
            get;
            private set;
        }
    }
}

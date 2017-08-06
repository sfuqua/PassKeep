// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;

namespace PassKeep.Lib.Contracts.Models
{
    public interface  IKeePassTimes : IKeePassSerializable
    {
        DateTime? CreationTime { get; }
        bool Expires { get; set; }
        DateTime? ExpiryTime { get; set; }
        DateTime? LastAccessTime { get; set; }
        DateTime? LastModificationTime { get; set; }
        DateTime? LocationChanged { get; set; }
        int UsageCount { get; set; }

        /// <summary>
        /// Updates the values of this instance to the specified copy.
        /// </summary>
        /// <param name="times">The master copy to synchronize values to.</param>
        void SyncTo(IKeePassTimes times);
        IKeePassTimes Clone();
    }
}

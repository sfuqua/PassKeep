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

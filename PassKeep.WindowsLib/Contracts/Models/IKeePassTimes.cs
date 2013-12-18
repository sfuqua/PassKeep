using System;

namespace PassKeep.Lib.Contracts.Models
{
    public interface IKeePassTimes : IKeePassSerializable
    {
        DateTime CreationTime { get; }
        bool Expires { get; set; }
        DateTime ExpiryTime { get; set; }
        DateTime LastAccessTime { get; set; }
        DateTime LastModificationTime { get; set; }
        DateTime LocationChanged { get; set; }
        int UsageCount { get; set; }

        IKeePassTimes Clone();
    }
}

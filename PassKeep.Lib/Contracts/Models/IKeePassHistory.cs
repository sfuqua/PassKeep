using System.Collections.Generic;

namespace PassKeep.Lib.Contracts.Models
{
    public interface IKeePassHistory : IKeePassSerializable
    {
        /// <summary>
        /// The entries in this history.
        /// </summary>
        IReadOnlyList<IKeePassEntry> Entries { get; }

        /// <summary>
        /// Clones this history.
        /// </summary>
        /// <returns>A duplicate of this history with all entries cloned.</returns>
        IKeePassHistory Clone();

        /// <summary>
        /// Adds an entry to this history.
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        void Add(IKeePassEntry entry);
    }
}

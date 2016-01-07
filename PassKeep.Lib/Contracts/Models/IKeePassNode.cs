using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PassKeep.Lib.Contracts.Models
{
    /// <summary>
    /// A child in the tree that represents a document.
    /// Could be a child or an entry.
    /// </summary>
    public interface IKeePassNode : IKeePassSerializable, INotifyPropertyChanged
    {
        /// <summary>
        /// A unique identifier.
        /// </summary>
        KeePassUuid Uuid { get; set; }

        /// <summary>
        /// A title or description.
        /// </summary>
        IProtectedString Title { get; set; }

        /// <summary>
        /// Notes associated with the child.
        /// </summary>
        IProtectedString Notes { get; set; }
        
        /// <summary>
        /// The parent of this child.
        /// </summary>
        IKeePassGroup Parent { get;}

        /// <summary>
        /// The creation/edited/etc times associated with this child.
        /// </summary>
        IKeePassTimes Times { get; }

        /// <summary>
        /// A number identifying the KeePass icon of this child.
        /// </summary>
        int IconID { get; set; }
        KeePassUuid CustomIconUuid { get; }

        bool HasAncestor(IKeePassGroup group);

        /// <summary>
        /// Used to facilate searches.
        /// </summary>
        /// <param name="query">A search string.</param>
        /// <returns>Whether this entity matches the query.</returns>
        bool MatchesQuery(string query);
    }
}

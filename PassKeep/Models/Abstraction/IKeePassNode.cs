using System;
using System.Collections.Generic;

namespace PassKeep.Models.Abstraction
{
    /// <summary>
    /// A node in the tree that represents a database.
    /// Could be a node or an entry.
    /// </summary>
    public interface IKeePassNode : IKeePassSerializable
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
        /// Notes associated with the node.
        /// </summary>
        IProtectedString Notes { get; set; }
        
        /// <summary>
        /// The parent of this node.
        /// </summary>
        IKeePassGroup Parent { get;}

        /// <summary>
        /// The creation/edited/etc times associated with this node.
        /// </summary>
        KdbxTimes Times { get; }

        /// <summary>
        /// A number identifying the KeePass icon of this node.
        /// </summary>
        int IconID { get; }
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

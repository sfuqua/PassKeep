using System.ComponentModel;

namespace PassKeep.Models.Abstraction
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
        IKeePassGroup Parent { get; }

        /// <summary>
        /// The creation/edited/etc times associated with this child.
        /// </summary>
        KdbxTimes Times { get; }

        /// <summary>
        /// A number identifying the KeePass icon of this child.
        /// </summary>
        int IconID { get; set; }
        KeePassUuid CustomIconUuid { get; }

        /// <summary>
        /// <paramref name="newParent"/> adopts this node as its own.
        /// </summary>
        /// <param name="newParent">The group that will adopt this node.</param>
        void Reparent(IKeePassGroup newParent);

        bool HasAncestor(IKeePassGroup group);

        /// <summary>
        /// Used to facilate searches.
        /// </summary>
        /// <param name="query">A search string.</param>
        /// <returns>Whether this entity matches the query.</returns>
        bool MatchesQuery(string query);
    }
}

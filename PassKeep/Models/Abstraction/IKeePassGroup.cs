using System.Collections.ObjectModel;

namespace PassKeep.Models.Abstraction
{
    public interface IKeePassGroup : IKeePassNode
    {
        bool? EnableSearching { get; set; }
        bool IsExpanded { get; }

        ObservableCollection<IKeePassNode> Children { get; }
        ObservableCollection<IKeePassGroup> Groups { get; }
        ObservableCollection<IKeePassEntry> Entries { get; }

        bool HasDescendant(IKeePassNode node);

        string DefaultAutoTypeSequence { get; }
        bool? EnableAutoType { get; }
        KeePassUuid LastTopVisibleEntry { get; }

        /// <summary>
        /// Whether entries are searchable in this group, accounting for inheritance.
        /// </summary>
        /// <returns>Whether entries are searchable in this group, accounting for inheritance.</returns>
        bool IsSearchingPermitted();

        void SyncTo(IKeePassGroup template, bool updateModificationTime = true);
        IKeePassGroup Clone();

        /// <summary>
        /// Attempts to locate the given node in the tree, and returns whether 
        /// this is a legal adoption. A group cannot adopt itself or its direct ancestors.
        /// i.e., cycles are illegal.
        /// </summary>
        /// <param name="encodedUuid">The encoded Uuid of the node to adopt.</param>
        /// <returns>Whether adoption would be successful.</returns>
        bool CanAdopt(string encodedUuid);

        /// <summary>
        /// Attempts to locate the given node in the tree, and adopts it if possible.
        /// </summary>
        /// <param name="encodedUuid">The encoded Uuid of the node to adopt.</param>
        /// <returns>Whether adoption was successful.</returns>
        bool TryAdopt(string encodedUuid);
    }
}

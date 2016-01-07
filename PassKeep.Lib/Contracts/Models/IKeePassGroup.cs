using System.Collections.ObjectModel;

namespace PassKeep.Lib.Contracts.Models
{
    public interface IKeePassGroup : IKeePassNode
    {
        bool? EnableSearching { get; set; }
        bool IsExpanded { get; }

        ObservableCollection<IKeePassNode> Children { get; }

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
    }
}

using System.Collections.ObjectModel;

namespace PassKeep.Models.Abstraction
{
    public interface IKeePassGroup : IKeePassNode
    {
        bool? EnableSearching { get; }
        bool IsExpanded { get; }

        ObservableCollection<IKeePassNode> Children { get; }
        ObservableCollection<IKeePassGroup> Groups { get; }
        ObservableCollection<IKeePassEntry> Entries { get; }

        bool HasDescendant(IKeePassNode node);

        string DefaultAutoTypeSequence { get; }
        bool? EnableAutoType { get; }
        KeePassUuid LastTopVisibleEntry { get; }

        void Update(IKeePassGroup template, bool updateModificationTime = true);
        IKeePassGroup Clone();
    }
}

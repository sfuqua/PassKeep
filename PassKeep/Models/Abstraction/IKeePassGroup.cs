using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Models.Abstraction
{
    public interface IKeePassGroup : IKeePassNode
    {
        bool? EnableSearching { get; }
        bool IsExpanded { get; }

        IList<IKeePassGroup> Groups { get; }
        IList<IKeePassEntry> Entries { get; }

        string DefaultAutoTypeSequence { get; }
        bool? EnableAutoType { get; }
        KeePassUuid LastTopVisibleEntry { get; }

        void Update(IKeePassGroup template, bool updateModificationTime = true);
        IKeePassGroup Clone();
    }
}

using PassKeep.Lib.Contracts.Models;
using System.Collections.Generic;

namespace PassKeep.Lib.Models
{
    public static class NodeComparer
    {
        /// <summary>
        /// A comparer that sorts groups before entries.
        /// </summary>
        public static readonly Comparer<IKeePassNode> GroupFirstComparer = Comparer<IKeePassNode>.Create(
            (nodeX, nodeY) =>
            {
                if (nodeX is IKeePassGroup)
                {
                    if (nodeY is IKeePassGroup)
                    {
                        // Both are groups
                        return 0;
                    }

                    // X is a group and Y is an entry
                    return -1;
                }
                else
                {
                    if (nodeY is IKeePassGroup)
                    {
                        // X is an entry and Y is a group
                        return 1;
                    }

                    // Both are entries
                    return 0;
                }
            }
        );
    }
}

using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Contracts.Models;
using System;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel that wraps an <see cref="IKeePassNode"/>.
    /// </summary>
    public class DatabaseNodeViewModel : IDatabaseNodeViewModel
    {
        /// <summary>
        /// Initializes the proxy.
        /// </summary>
        /// <param name="node"></param>
        public DatabaseNodeViewModel(IKeePassNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            this.Node = node;
        }

        /// <summary>
        /// Provides access to the wrapped node.
        /// </summary>
        public IKeePassNode Node
        {
            get;
            private set;
        }
    }
}

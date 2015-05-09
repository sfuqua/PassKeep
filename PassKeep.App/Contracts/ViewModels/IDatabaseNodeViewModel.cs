using PassKeep.Lib.Contracts.Models;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Wraps an <see cref="IKeePassNode"/> with some additional View-related logic.
    /// </summary>
    public interface IDatabaseNodeViewModel
    {
        /// <summary>
        /// Provides access to the wrapped node.
        /// </summary>
        IKeePassNode Node { get; }
    }
}

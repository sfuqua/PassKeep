using PassKeep.Lib.Contracts.Models;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// A ViewModel representing a detailed view of a Node (e.g., an entry or group).
    /// </summary>
    /// <typeparam name="T">The type of Node.</typeparam>
    public interface INodeDetailsViewModel<T> : IViewModel
        where T : IKeePassNode
    {
        /// <summary>
        /// Whether or not editing is enabled for the View.
        /// </summary>
        bool IsReadOnly
        {
            get;
        }

        /// <summary>
        /// The Node being viewed in detail.
        /// </summary>
        T Item
        {
            get;
        }
    }
}

using PassKeep.Lib.Contracts.Models;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Interface for a ViewModel that represents a detailed, editable view of an IKeePassEntry.
    /// </summary>
    public interface IEntryDetailsViewModel : INodeDetailsViewModel<IKeePassEntry>
    {
    }
}

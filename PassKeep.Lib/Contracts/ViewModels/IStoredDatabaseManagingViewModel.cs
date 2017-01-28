using PassKeep.Models;
using System.Collections.ObjectModel;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Abstracts shared functionality regarding managing <see cref="StoredFileDescriptor"/> instances.
    /// </summary>
    public interface IStoredDatabaseManagingViewModel : IViewModel
    {
        /// <summary>
        /// Provides access to a list of recently accessed databases, for easy opening.
        /// </summary>
        ReadOnlyObservableCollection<StoredFileDescriptor> StoredFiles
        {
            get;
        }
    }
}

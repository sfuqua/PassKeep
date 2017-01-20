using PassKeep.Lib.EventArgClasses;
using PassKeep.Models;
using System.Collections.ObjectModel;
using Windows.Foundation;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Abstracts shared functionality regarding managing <see cref="StoredFileDescriptor"/> instances.
    /// </summary>
    public interface IStoredDatabaseManagingViewModel : IViewModel
    {
        /// <summary>
        /// Fired when the View should consent to deleting a stored file descriptor.
        /// </summary>
        event TypedEventHandler<IStoredDatabaseManagingViewModel, RequestForgetDescriptorEventArgs> RequestForgetDescriptor;

        /// <summary>
        /// Fired when the View should provide a file to update a stored descriptor.
        /// </summary>
        event TypedEventHandler<IStoredDatabaseManagingViewModel, RequestUpdateDescriptorEventArgs> RequestUpdateDescriptor;

        /// <summary>
        /// Provides access to a list of recently accessed databases, for easy opening.
        /// </summary>
        ReadOnlyObservableCollection<StoredFileDescriptor> StoredFiles
        {
            get;
        }
    }
}

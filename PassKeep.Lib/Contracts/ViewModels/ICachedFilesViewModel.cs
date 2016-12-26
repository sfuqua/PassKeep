using PassKeep.Models;
using SariphLib.Mvvm;
using System.Collections.ObjectModel;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Represents a ViewModel over a list of local cached files, allowing
    /// them to be managed/deleted.
    /// </summary>
    public interface ICachedFilesViewModel : IViewModel
    {
        /// <summary>
        /// A command for deleting a specific file.
        /// </summary>
        IAsyncCommand DeleteFileAsyncCommand { get; }

        /// <summary>
        /// A command for deleting all files.
        /// </summary>
        IAsyncCommand DeleteAllAsyncCommand { get; }

        /// <summary>
        /// A collection of files that are stored.
        /// </summary>
        ObservableCollection<StoredFileDescriptor> CachedFiles { get; }
    }
}

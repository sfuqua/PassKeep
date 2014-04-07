using PassKeep.Lib.Contracts.Models;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Data;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IDatabaseArrangementViewModel : IViewModel
    {
        /// <summary>
        /// The current DatabaseSortMode used by this ViewModel.
        /// </summary>
        DatabaseSortMode SortMode
        {
            get;
            set;
        }

        /// <summary>
        /// A listing of all known, available sort modes.
        /// </summary>
        ReadOnlyObservableCollection<DatabaseSortMode> AvailableSortModes
        {
            get;
        }

        /// <summary>
        /// Sorts and returns the children of the specified group based on the current arrangement mode.
        /// </summary>
        /// <param name="currentGroup">The group whose children to query.</param>
        /// <param name="viewSource">A CollectionViewSource to populate as a result of the arrangement.</param>
        /// <returns>A list filtered and sorted based on the current mode.</returns>
        void ArrangeChildren(IKeePassGroup currentGroup, ref CollectionViewSource viewSource);
    }
}

using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.Dom;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IDatabaseViewModel : IViewModel
    {
        /// <summary>
        /// The navigation ViewModel for the database.
        /// </summary>
        /// <remarks>
        /// This is responsible for tracking "where" the user is in the database.
        /// </remarks>
        IDatabaseNavigationViewModel NavigationViewModel { get; }

        /// <summary>
        /// The actual KdbxDocument represented by the ViewModel.
        /// </summary>
        KdbxDocument Document { get; }

        /// <summary>
        /// Attempts to save the current state of the database to storage.
        /// </summary>
        /// <returns>A Task representing whether the save was successful.</returns>
        Task<bool> TrySave();

        /// <summary>
        /// Gets a collection of queryable IKeePassNodes for search purposes.
        /// </summary>
        /// <returns>A collection of all IKeePassNodes (entries and groups) that are visible to searches.</returns>
        ICollection<IKeePassNode> GetAllSearchableNodes();
    }
}

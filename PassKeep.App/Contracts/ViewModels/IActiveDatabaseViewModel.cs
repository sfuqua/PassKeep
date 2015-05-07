using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// A ViewModel that represents an active, unlocked database.
    /// </summary>
    public interface IActiveDatabaseViewModel
    {
        /// <summary>
        /// The navigation ViewModel for the document.
        /// </summary>
        /// <remarks>
        /// This is responsible for tracking "where" the user is in the document.
        /// </remarks>
        IDatabaseNavigationViewModel NavigationViewModel { get; }

        /// <summary>
        /// The actual KdbxDocument represented by the ViewModel.
        /// </summary>
        KdbxDocument Document { get; }
    }
}

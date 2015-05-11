namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// ViewModel for a view over an unlocked database, regardless of the details of the view.
    /// </summary>
    public interface IDatabaseParentViewModel : IDatabasePersistenceViewModel, IActiveDatabaseViewModel
    {
        /// <summary>
        /// Generates an <see cref="IDatabaseViewModel"/> based on current state.
        /// </summary>
        /// <returns>A ViewModel over the database tree.</returns>
        IDatabaseViewModel GetDatabaseViewModel();
    }
}

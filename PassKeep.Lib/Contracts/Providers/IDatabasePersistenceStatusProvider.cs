using System.ComponentModel;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Allows the app to monitor whether a save is in progress.
    /// </summary>
    public interface IDatabasePersistenceStatusProvider : INotifyPropertyChanged
    {
        /// <summary>
        /// Whether a save is in progress.
        /// </summary>
        bool IsSaving { get; }
    }
}

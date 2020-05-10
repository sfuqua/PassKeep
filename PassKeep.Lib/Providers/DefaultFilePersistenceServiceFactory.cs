using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Services;
using SariphLib.Mvvm;

namespace PassKeep.Lib.Providers
{
    /// <summary>
    /// Generates services that know how to save databases to a file.
    /// </summary>
    public class DefaultFilePersistenceServiceFactory : BindableBase, IDatabasePersistenceServiceFactory, IDatabasePersistenceStatusProvider
    {
        private readonly ISyncContext syncContext;

        /// <summary>
        /// Inits the factory.
        /// </summary>
        /// <param name="syncContext">Synchronizes access to the UI thread.</param>
        public DefaultFilePersistenceServiceFactory(ISyncContext syncContext)
        {
            this.syncContext = syncContext;
        }

        public bool IsSaving
        {
            get;
            private set;
        }

        /// <summary>
        /// Assembles a service.
        /// </summary>
        /// <param name="writer">Serializes.</param>
        /// <param name="settings">How to serialize.</param>
        /// <param name="candidate">What to serialize.</param>
        /// <param name="canSave">Whether serialization is possible.</param>
        /// <returns></returns>
        public IDatabasePersistenceService Assemble(IKdbxWriter writer, IDatabaseSettingsProvider settings, IDatabaseCandidate candidate, bool canSave)
        {
            DefaultFilePersistenceService service = new DefaultFilePersistenceService(
                writer,
                settings,
                candidate,
                this.syncContext,
                canSave
            );

            service.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(service.IsSaving))
                {
                    // TODO: Evaluate whether this needs to be debounced or aggregated
                    bool was = IsSaving;
                    IsSaving = service.IsSaving;
                    if (was != IsSaving)
                    {
                        OnPropertyChanged(nameof(IsSaving));
                    }
                }
            };

            return service;
        }
    }
}

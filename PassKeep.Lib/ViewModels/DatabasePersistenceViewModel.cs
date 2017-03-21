using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using System;
using System.Threading.Tasks;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel that is capable of writing to the document.
    /// </summary>
    public abstract class DatabasePersistenceViewModel : AbstractViewModel, IDatabasePersistenceViewModel
    {
        private KdbxDocument document;

        /// <summary>
        /// Initializes the ViewModel base.
        /// </summary>
        /// <param name="document">The document that will be saved.</param>
        /// <param name="persistenceService">The service to use for document writing.</param>
        protected DatabasePersistenceViewModel(KdbxDocument document, IDatabasePersistenceService persistenceService)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (persistenceService == null)
            {
                throw new ArgumentNullException(nameof(persistenceService));
            }

            this.document = document;
            PersistenceService = persistenceService;
        }

        /// <summary>
        /// The persistence service used to save the database.
        /// </summary>
        public IDatabasePersistenceService PersistenceService
        {
            get;
            private set;
        }

        /// <summary>
        /// Attempts to save the current state of the document to storage.
        /// </summary>
        /// <returns>A Task representing whether the save was successful.</returns>
        public virtual Task Save()
        {
            return PersistenceService.Save(this.document);
        }
    }
}

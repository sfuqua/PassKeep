using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Mvvm;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel that is capable of writing to the document.
    /// </summary>
    public abstract class DatabasePersistenceViewModel : BindableBase, IDatabasePersistenceViewModel
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
                throw new ArgumentNullException("document");
            }

            if (persistenceService == null)
            {
                throw new ArgumentNullException("persistenceService");
            }

            this.document = document;
            this.PersistenceService = persistenceService;
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
        /// Raised when a new save operation has begun.
        /// </summary>
        public event EventHandler<CancellableEventArgs> StartedSave;
        private void RaiseStartedSave(CancellationTokenSource cts)
        {
            if (StartedSave != null)
            {
                StartedSave(this, new CancellableEventArgs(cts));
            }
        }

        /// <summary>
        /// Raised when a save operation has stopped for any reason.
        /// </summary>
        public event EventHandler StoppedSave;
        private void RaiseStoppedSave()
        {
            if (StoppedSave != null)
            {
                StoppedSave(this, new EventArgs());
            }
        }

        /// <summary>
        /// Attempts to save the current state of the document to storage.
        /// </summary>
        /// <returns>A Task representing whether the save was successful.</returns>
        public virtual async Task<bool> TrySave()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            RaiseStartedSave(cts);
            bool success = await this.PersistenceService.Save(this.document, cts.Token);
            RaiseStoppedSave();

            return success;
        }
    }
}

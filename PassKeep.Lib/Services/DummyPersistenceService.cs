using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.KeePass.Dom;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// A "Service" that always no-ops for document persistence.
    /// </summary>
    public class DummyPersistenceService : IDatabasePersistenceService
    {
        /// <summary>
        /// The dummy service can always save.
        /// </summary>
        public bool CanSave
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="document">The KdbxDocument to persist.</param>
        /// <param name="token">A CancellationToken for the operation.</param>
        /// <returns>A Task that will evaluate to true if the token hasn't been cancelled.</returns>
        public Task<bool> Save(KdbxDocument document, CancellationToken token)
            => Task.Run(() => !token.IsCancellationRequested);
    }
}

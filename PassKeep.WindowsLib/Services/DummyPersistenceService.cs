using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.KeePass.Dom;
using System.Threading;
using System.Threading.Tasks;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// A "Service" that always no-ops for database persistence.
    /// </summary>
    public class DummyPersistenceService : IDatabasePersistenceService
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="document">The KdbxDocument to persist.</param>
        /// <param name="token">An unused CancellationToken.</param>
        /// <returns>A Task that will evaluate to true.</returns>
        public Task<bool> Save(KdbxDocument document, CancellationToken token)
        {
            return Task.Run(() => true);
        }
    }
}

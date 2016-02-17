using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Models;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Lib.Providers
{
    /// <summary>
    /// A factory for assembling <see cref="StorageFileDatabaseCandidate"/> instances.
    /// </summary>
    public class StorageFileDatabaseCandidateFactory : IDatabaseCandidateFactory
    {
        /// <summary>
        /// Assembles a <see cref="StorageFileDatabaseCandidate"/> that wraps
        /// <paramref name="file"/> and initializes the cached file for reading.
        /// </summary>
        /// <param name="file">The file to wrap.</param>
        /// <returns>An initialized <see cref="StorageFileDatabaseCandidate"/>.</returns>
        public async Task<IDatabaseCandidate> AssembleAsync(IStorageFile file)
        {
            StorageFileDatabaseCandidate candidate = new StorageFileDatabaseCandidate(file);
            await candidate.GenerateReadOnlyCachedCopyAsync();

            return candidate;
        }
    }
}

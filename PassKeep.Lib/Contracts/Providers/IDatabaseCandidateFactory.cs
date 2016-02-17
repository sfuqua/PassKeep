using PassKeep.Contracts.Models;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// An interface for assembling <see cref="IDatabaseCandidate"/> instances.
    /// </summary>
    public interface IDatabaseCandidateFactory
    {
        /// <summary>
        /// Asynchronously manufactures an <see cref="IDatabaseCandidate"/>
        /// from <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to wrap with a candidate.</param>
        /// <returns>A task that resolves to an <see cref="IDatabaseCandidate"/>.</returns>
        Task<IDatabaseCandidate> AssembleAsync(IStorageFile file);
    }
}

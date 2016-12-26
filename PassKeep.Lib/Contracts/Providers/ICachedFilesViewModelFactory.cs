using PassKeep.Lib.Contracts.ViewModels;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// A helper for constructing activated <see cref="ICachedFilesViewModel"/> instances.
    /// </summary>
    public interface ICachedFilesViewModelFactory
    {
        /// <summary>
        /// Asynchronously constructs and activates an 
        /// <see cref="ICachedFilesViewModel"/>.
        /// </summary>
        /// <returns>A task that completes when the ViewModel is ready for use.</returns>
        Task<ICachedFilesViewModel> AssembleAsync();
    }
}

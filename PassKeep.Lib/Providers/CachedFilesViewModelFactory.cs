using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.ViewModels;
using System;
using System.Threading.Tasks;

namespace PassKeep.Lib.Providers
{
    /// <summary>
    /// A helper for constructing activated <see cref="CachedFilesViewModel"/> instances.
    /// </summary>
    public sealed class CachedFilesViewModelFactory : ICachedFilesViewModelFactory
    {
        private readonly IFileProxyProvider proxyProvider;

        public CachedFilesViewModelFactory(IFileProxyProvider proxyProvider)
        {
            if (proxyProvider == null)
            {
                throw new ArgumentNullException(nameof(proxyProvider));
            }

            this.proxyProvider = proxyProvider;
        }

        /// <summary>
        /// Asynchronously constructs and activates an 
        /// <see cref="ICachedFilesViewModel"/>.
        /// </summary>
        /// <returns>A task that completes when the ViewModel is ready for use.</returns>
        public async Task<ICachedFilesViewModel> AssembleAsync()
        {
            CachedFilesViewModel viewModel = new CachedFilesViewModel(
                this.proxyProvider
            );

            await viewModel.ActivateAsync();
            return viewModel;
        }
    }
}

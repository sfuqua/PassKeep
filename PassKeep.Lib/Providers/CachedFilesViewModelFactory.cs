using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
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
        private readonly IDatabaseAccessList accessList;
        private readonly IFileExportService exportService;
        private readonly IFileProxyProvider proxyProvider;

        public CachedFilesViewModelFactory(IDatabaseAccessList accessList, IFileExportService exportService, IFileProxyProvider proxyProvider)
        {
            if (accessList == null)
            {
                throw new ArgumentNullException(nameof(accessList));
            }

            if (exportService == null)
            {
                throw new ArgumentNullException(nameof(exportService));
            }

            if (proxyProvider == null)
            {
                throw new ArgumentNullException(nameof(proxyProvider));
            }

            this.accessList = accessList;
            this.exportService = exportService;
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
                this.accessList,
                this.exportService,
                this.proxyProvider
            );

            await viewModel.ActivateAsync();
            return viewModel;
        }
    }
}

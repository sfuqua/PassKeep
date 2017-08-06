// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
        private readonly IUserPromptingService deletePrompter;
        private readonly IUserPromptingService updatePrompter;
        private readonly IFileAccessService fileService;

        public CachedFilesViewModelFactory(
            IDatabaseAccessList accessList,
            IFileExportService exportService,
            IFileProxyProvider proxyProvider,
            IUserPromptingService deletePrompter,
            IUserPromptingService updatePrompter,
            IFileAccessService fileService
        )
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

            if (deletePrompter == null)
            {
                throw new ArgumentNullException(nameof(deletePrompter));
            }

            if (updatePrompter == null)
            {
                throw new ArgumentNullException(nameof(updatePrompter));
            }

            if (fileService == null)
            {
                throw new ArgumentNullException(nameof(fileService));
            }

            this.accessList = accessList;
            this.exportService = exportService;
            this.proxyProvider = proxyProvider;
            this.deletePrompter = deletePrompter;
            this.updatePrompter = updatePrompter;
            this.fileService = fileService;
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
                this.proxyProvider,
                this.deletePrompter,
                this.updatePrompter,
                this.fileService
            );

            await viewModel.ActivateAsync();
            return viewModel;
        }
    }
}

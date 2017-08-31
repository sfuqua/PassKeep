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
            this.accessList = accessList ?? throw new ArgumentNullException(nameof(accessList));
            this.exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
            this.proxyProvider = proxyProvider ?? throw new ArgumentNullException(nameof(proxyProvider));
            this.deletePrompter = deletePrompter ?? throw new ArgumentNullException(nameof(deletePrompter));
            this.updatePrompter = updatePrompter ?? throw new ArgumentNullException(nameof(updatePrompter));
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
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

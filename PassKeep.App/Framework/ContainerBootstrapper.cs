// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using Microsoft.Practices.Unity;
using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Providers;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using PassKeep.Models;
using SariphLib.Diagnostics;
using SariphLib.Mvvm;
using System;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace PassKeep.Framework
{
    public class ContainerBootstrapper
    {
        public static readonly Guid EtwProviderGuid = new Guid("BF46B8E2-7986-4D7B-B5B1-5625283FC1D3");

        public static void RegisterTypes(IUnityContainer container)
        {
            IResourceProvider resourceProvider = new ResourceProvider(ResourceLoader.GetForViewIndependentUse());

            IUserPromptingService cachedFileDeletePrompter = new MessageDialogService(
                resourceProvider.GetString("DeletePromptTitle"),
                resourceProvider.GetString("ForgetCachedPromptContent"),
                resourceProvider.GetString("Yes"),
                resourceProvider.GetString("No")
            );

            IUserPromptingService cachedFileUpdatePrompter = new MessageDialogService(
                resourceProvider.GetString("ReplaceCachePromptTitle"),
                resourceProvider.GetString("ReplaceCachePromptContent"),
                resourceProvider.GetString("Yes"),
                resourceProvider.GetString("No")
            );

            // Providers
            container
                .RegisterType<ICryptoRngProvider, CryptographicBufferRngProvider>(new ContainerControlledLifetimeManager())
                .RegisterType<ISettingsProvider, RoamingAppDataSettingsProvider>(new ContainerControlledLifetimeManager())
                .RegisterType<IClipboardProvider, WindowsClipboardProvider>(new ContainerControlledLifetimeManager())
                .RegisterType<ICredentialStorageProvider, PasswordVaultCredentialProvider>(new ContainerControlledLifetimeManager())
                .RegisterType<ITimerFactory, ThreadPoolTimerFactory>()
                .RegisterType<IKdbxWriterFactory, KdbxWriterFactory>()
                .RegisterType<ICachedFilesViewModelFactory, CachedFilesViewModelFactory>(
                    new InjectionConstructor(
                        typeof(IDatabaseAccessList),
                        typeof(IFileExportService),
                        typeof(IFileProxyProvider),
                        cachedFileDeletePrompter,
                        cachedFileUpdatePrompter,
                        typeof(IFileAccessService)
                    )
                )
                .RegisterInstance<IDatabaseAccessList>(
                    new DatabaseAccessList(StorageApplicationPermissions.MostRecentlyUsedList)
                )
                .RegisterInstance<IResourceProvider>(
                    new ResourceProvider(ResourceLoader.GetForViewIndependentUse())
                )
                .RegisterInstance<IFileProxyProvider>(
                    new FileProxyProvider(ApplicationData.Current.LocalCacheFolder)
                );

            // Services
            EtwTraceLogger logger = new EtwTraceLogger("PassKeep.Etw", EtwProviderGuid);
            DebugHelper.Logger = logger;

            container
                .RegisterType<IPasswordGenerationService, PasswordGenerationService>(new ContainerControlledLifetimeManager())
                .RegisterType<IAppSettingsService, AppSettingsService>(new ContainerControlledLifetimeManager())
                .RegisterType<IDatabasePersistenceService, DefaultFilePersistenceService>()
                .RegisterType<ISensitiveClipboardService, SensitiveClipboardService>(new ContainerControlledLifetimeManager())
                .RegisterType<ITaskNotificationService, TaskNotificationService>(new ContainerControlledLifetimeManager())
                .RegisterType<IIdentityVerificationService, HelloBasedVerificationService>(new ContainerControlledLifetimeManager())
                .RegisterType<IFileExportService, FileExportService>(new ContainerControlledLifetimeManager())
                .RegisterType<ISyncContext, DispatcherContext>(new ContainerControlledLifetimeManager(), new InjectionConstructor())
                .RegisterInstance<IFileAccessService>(
                    new FilePickerService(".kdbx", resourceProvider.GetString(BasePassKeepPage.KdbxFileDescResourceKey)),
                    new ContainerControlledLifetimeManager()
                )
                .RegisterInstance<IEventLogger>(logger)
                .RegisterInstance<IEventTracer>(logger);

            // ViewModels
            container
                .RegisterType<IRootViewModel, RootViewModel>()
                .RegisterType<ISearchViewModel, SearchViewModel>()
                .RegisterType<IPasswordGenViewModel, PasswordGenViewModel>()
                .RegisterType<IClipboardClearTimerViewModel, SettingsBasedClipboardClearViewModel>(new ContainerControlledLifetimeManager())
                .RegisterType<IDashboardViewModel, DashboardViewModel>(
                    new InjectionConstructor(
                        typeof(IDatabaseAccessList),
                        typeof(IMotdProvider),
                        typeof(IFileProxyProvider),
                        typeof(IFileExportService),
                        cachedFileDeletePrompter,
                        cachedFileUpdatePrompter,
                        typeof(IFileAccessService)
                    )
                )
                .RegisterType<IDatabaseUnlockViewModel, DatabaseUnlockViewModel>()
                .RegisterType<IDatabaseCreationViewModel, DatabaseCreationViewModel>()
                .RegisterType<IDatabaseNavigationViewModel, DatabaseNavigationViewModel>()
                .RegisterType<IDatabaseParentViewModel, DatabaseParentViewModel>()
                .RegisterType<IDiagnosticTraceButtonViewModel, DiagnosticTraceButtonViewModel>()
                .RegisterType<IHelpViewModel, HelpViewModel>()
                .RegisterType<IAppSettingsViewModel, AppSettingsViewModel>()
                .RegisterType<ISavedCredentialsViewModelFactory, SavedCredentialViewModelFactory>(new ContainerControlledLifetimeManager())
                .RegisterType<IDatabaseCandidateFactory, StorageFileDatabaseCandidateFactory>(new ContainerControlledLifetimeManager());

            // KeePass
            container
                .RegisterType<IKdbxReader, KdbxReader>()
                .RegisterType<IKdbxWriter, KdbxWriter>();

            // Objects that need special consideration
            container
                .RegisterInstance<IMotdProvider>(
                    new ResourceBasedMotdProvider(
                        new ResourceProvider(ResourceLoader.GetForViewIndependentUse("Motd")),
                        container.Resolve<ISettingsProvider>(),
                        container.Resolve<IAppSettingsService>()
                    )
                )
                .RegisterInstance<IDiagnosticTraceButtonViewModel>(
                    new DiagnosticTraceButtonViewModel(
                        container.Resolve<IEventLogger>(),
                        container.Resolve<IEventTracer>(),
                        resourceProvider.GetString("StartTraceLabel"),
                        resourceProvider.GetString("StopTraceLabel")
                    )
                );
        }
    }
}

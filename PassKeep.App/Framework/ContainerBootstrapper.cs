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
using SariphLib.Mvvm;
using Windows.ApplicationModel.Resources;
using Windows.Storage.AccessCache;

namespace PassKeep.Framework
{
    public class ContainerBootstrapper
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            // Providers
            container
                .RegisterType<ICryptoRngProvider, CryptographicBufferRngProvider>(new ContainerControlledLifetimeManager())
                .RegisterType<ISettingsProvider, RoamingAppDataSettingsProvider>(new ContainerControlledLifetimeManager())
                .RegisterType<IClipboardProvider, WindowsClipboardProvider>(new ContainerControlledLifetimeManager())
                .RegisterType<ICredentialStorageProvider, PasswordVaultCredentialProvider>(new ContainerControlledLifetimeManager())
                .RegisterType<ITimerFactory, ThreadPoolTimerFactory>()
                .RegisterType<IKdbxWriterFactory, KdbxWriterFactory>()
                .RegisterInstance<IDatabaseAccessList>(
                    new DatabaseAccessList(StorageApplicationPermissions.MostRecentlyUsedList)
                )
                .RegisterInstance<IResourceProvider>(
                    new ResourceProvider(ResourceLoader.GetForViewIndependentUse())
                );

            // Services
            container
                .RegisterType<IPasswordGenerationService, PasswordGenerationService>(new ContainerControlledLifetimeManager())
                .RegisterType<IAppSettingsService, AppSettingsService>(new ContainerControlledLifetimeManager())
                .RegisterType<IDatabasePersistenceService, DefaultFilePersistenceService>()
                .RegisterType<ISensitiveClipboardService, SensitiveClipboardService>(new ContainerControlledLifetimeManager())
                .RegisterType<ITaskNotificationService, TaskNotificationService>(new ContainerControlledLifetimeManager())
                .RegisterType<IIdentityVerificationService, HelloBasedVerificationService>(new ContainerControlledLifetimeManager())
                .RegisterType<ISyncContext, DispatcherContext>(new ContainerControlledLifetimeManager(), new InjectionConstructor());

            // ViewModels
            container
                .RegisterType<IRootViewModel, RootViewModel>()
                .RegisterType<ISearchViewModel, SearchViewModel>()
                .RegisterType<IPasswordGenViewModel, PasswordGenViewModel>()
                .RegisterType<IClipboardClearTimerViewModel, SettingsBasedClipboardClearViewModel>(new ContainerControlledLifetimeManager())
                .RegisterType<IDashboardViewModel, DashboardViewModel>()
                .RegisterType<IDatabaseUnlockViewModel, DatabaseUnlockViewModel>()
                .RegisterType<IDatabaseCreationViewModel, DatabaseCreationViewModel>()
                .RegisterType<IDatabaseNavigationViewModel, DatabaseNavigationViewModel>()
                .RegisterType<IDatabaseParentViewModel, DatabaseParentViewModel>()
                .RegisterType<IAppSettingsViewModel, AppSettingsViewModel>();

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
                );
        }
    }
}

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
                .RegisterInstance<IDatabaseAccessList>(
                    new DatabaseAccessList(StorageApplicationPermissions.MostRecentlyUsedList)
                )
                .RegisterInstance<ResourceLoader>(
                    ResourceLoader.GetForViewIndependentUse()
                );

            // Services
            container
                .RegisterType<IPasswordGenerationService, PasswordGenerationService>(new ContainerControlledLifetimeManager())
                .RegisterType<IAppSettingsService, AppSettingsService>(new ContainerControlledLifetimeManager())
                .RegisterType<IDatabasePersistenceService, DefaultFilePersistenceService>();

            // ViewModels
            container
                .RegisterType<IRootViewModel, RootViewModel>()
                .RegisterType<ISearchViewModel, SearchViewModel>()
                .RegisterType<IPasswordGenViewModel, PasswordGenViewModel>()
                .RegisterType<IClipboardClearTimerViewModel, SettingsBasedClipboardClearViewModel>(new ContainerControlledLifetimeManager())
                .RegisterType<IDashboardViewModel, DashboardViewModel>()
                .RegisterType<IDatabaseUnlockViewModel, DatabaseUnlockViewModel>()
                .RegisterType<IDatabaseNavigationViewModel, DatabaseNavigationViewModel>()
                .RegisterType<IDatabaseParentViewModel, DatabaseParentViewModel>();

            // KeePass
            container
                .RegisterType<IKdbxReader, KdbxReader>()
                .RegisterType<IKdbxWriter, KdbxWriter>();
        }
    }
}

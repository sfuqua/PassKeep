using Microsoft.Practices.Unity;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Providers;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using PassKeep.ViewModels;

namespace PassKeep.Framework
{
    public class ContainerBootstrapper
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            // Providers
            container
                .RegisterType<ICryptoRngProvider, CryptographicBufferRngProvider>(new ContainerControlledLifetimeManager())
                .RegisterType<ISettingsProvider, RoamingAppDataSettingsProvider>(new ContainerControlledLifetimeManager());

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
                .RegisterType<IDatabaseUnlockViewModel, DatabaseUnlockViewModel>()
                .RegisterType<IDatabaseNavigationViewModel, DatabaseNavigationViewModel>()
                .RegisterType<IDatabaseViewModel, DatabaseViewModel>()
                .RegisterType<IGroupDetailsViewModel, GroupDetailsViewModel>(
                    ContainerHelper.GroupDetailsViewNew,
                    new InjectionConstructor(new ResolvedParameter<IKeePassGroup>())
                )
                .RegisterType<IGroupDetailsViewModel, GroupDetailsViewModel>(
                    ContainerHelper.GroupDetailsViewExisting,
                    new InjectionConstructor(
                        new ResolvedParameter<IKeePassGroup>(), new ResolvedParameter<bool>()
                    )
                )
                .RegisterType<IEntryDetailsViewModel, EntryDetailsViewModel>(
                    ContainerHelper.EntryDetailsViewNew,
                    new InjectionConstructor(new ResolvedParameter<IKeePassEntry>())
                )
                .RegisterType<IEntryDetailsViewModel, EntryDetailsViewModel>(
                    ContainerHelper.EntryDetailsViewExisting,
                    new InjectionConstructor(
                        new ResolvedParameter<IKeePassEntry>(), new ResolvedParameter<bool>()
                    )
                );

            // KeePass
            container
                .RegisterType<IKdbxReader, KdbxReader>()
                .RegisterType<IKdbxWriter, KdbxWriter>();
        }
    }
}

using Microsoft.Practices.Unity;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Providers;
using PassKeep.Lib.Services;

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
                .RegisterType<IAppSettingsService, AppSettingsService>(new ContainerControlledLifetimeManager());

            // ViewModels
            container
                .RegisterType<IPasswordGenViewModel, IPasswordGenViewModel>();
        }
    }
}

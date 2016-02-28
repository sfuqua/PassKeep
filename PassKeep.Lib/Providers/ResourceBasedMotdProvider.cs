using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using System;
using Windows.ApplicationModel;

namespace PassKeep.Lib.Providers
{
    public sealed class ResourceBasedMotdProvider : IMotdProvider
    {
        /// <summary>
        /// Key to use for a <see cref="ISettingsProvider"/> to determine whether to show
        /// the MOTD.
        /// </summary>
        private const string SettingsKey = "MotdDisplayed";
        private readonly IResourceProvider resourceProvider;
        private readonly bool shouldDisplay;

        /// <summary>
        /// Instantiates the provider around the specified <see cref="IResourceProvider"/>.
        /// </summary>
        /// <param name="resources">The <see cref="IResourceProvider"/> from which to fetch
        /// MOTD strings.</param>
        /// <param name="settingsProvider">Used to resolve whether the MOTD should be displayed.</param>
        /// <param name="appSettings">Used to coordinate display with user settings.</param>
        public ResourceBasedMotdProvider(
            IResourceProvider resources,
            ISettingsProvider settingsProvider,
            IAppSettingsService appSettings
        )
        {
            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            if (settingsProvider == null)
            {
                throw new ArgumentNullException(nameof(settingsProvider));
            }

            if (appSettings == null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            this.resourceProvider = resources;

            if (appSettings.EnableMotd)
            {
                string motdVersion = resources.GetString("Revision");

                // Get the current app version and compare to what settings says was the last shown version.
                PackageVersion appVersion = Package.Current.Id.Version;
                string version = $"{appVersion.Major}.{appVersion.Minor}.{appVersion.Revision}";
                string lastShown = settingsProvider.Get<string>(SettingsKey, null);

                // Only show the MOTD if the version is correct and we haven't shown it yet on this build.
                this.shouldDisplay = (version != lastShown) && (version == motdVersion);
                if (this.shouldDisplay)
                {
                    settingsProvider.Set(SettingsKey, version);
                }
            }
            else
            {
                this.shouldDisplay = false;
            }
        }

        /// <summary>
        /// Evaluates whether the message-of-the-day should be displayed.
        /// </summary>
        public bool ShouldDisplay
        {
            get { return this.shouldDisplay; }
        }

        /// <summary>
        /// Provides the title of the message-of-the-day.
        /// </summary>
        /// <returns>A title.</returns>
        public string GetTitle()
        {
            return this.resourceProvider.GetString("Title");
        }

        /// <summary>
        /// Provides a body for the message-of-the-day.
        /// </summary>
        /// <returns>The body.</returns>
        public string GetBody()
        {
            return this.resourceProvider.GetString("Body");
        }

        /// <summary>
        /// Provides a description of an action that will dismiss the message-of-the-day.
        /// </summary>
        /// <returns>Dismissal text.</returns>
        public string GetDismiss()
        {
            return this.resourceProvider.GetString("Dismiss");
        }
    }
}

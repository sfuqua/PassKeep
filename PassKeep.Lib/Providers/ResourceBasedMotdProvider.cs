using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Models;
using System;
using Windows.ApplicationModel;

namespace PassKeep.Lib.Providers
{
    /// <summary>
    /// A provider that provides message-of-the-day information based on an <see cref="IResourceProvider"/>
    /// and settings interfaces. The MOTD will be displayed if one has not been shown for the current version,
    /// and settings specify that it is permitted by the user.
    /// </summary>
    public sealed class ResourceBasedMotdProvider : IMotdProvider
    {
        /// <summary>
        /// Key to use for a <see cref="ISettingsProvider"/> to determine whether to show
        /// the MOTD.
        /// </summary>
        public const string SettingsKey = "MotdDisplayed";

        /// <summary>
        /// Resource key to use to fetch the current version of the MOTD.
        /// </summary>
        public const string VersionResouceKey = "Revision";

        private readonly ISettingsProvider settingsProvider;
        private readonly string motdTitle, motdBody, motdDismissText;
        private readonly string motdVersion, appVersion;
        private bool shouldDisplay;

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

            this.settingsProvider = settingsProvider;

            if (appSettings.EnableMotd)
            {
                this.motdVersion = resources.GetString(VersionResouceKey);

                // Get the current app version and compare to what settings says was the last shown version.
                PackageVersion pkgVersion = Package.Current.Id.Version;
                this.appVersion = $"{pkgVersion.Major}.{pkgVersion.Minor}.{pkgVersion.Revision}";

                // Only show the MOTD if the version is correct and we haven't shown it yet on this build.
                this.shouldDisplay = ShouldDisplayBasedOnSettings();
            }
            else
            {
                this.shouldDisplay = false;
            }

            // Only load strings if we haven't shown this MOTD yet.
            if (this.shouldDisplay)
            {
                this.motdTitle = resources.GetString("Title");
                this.motdBody = resources.GetString("Body");
                this.motdDismissText = resources.GetString("Dismiss");
            }
        }

        /// <summary>
        /// Gets a <see cref="MessageOfTheDay"/> for immediate display.
        /// Subsequent calls will return a MOTD that is set not to display.
        /// </summary>
        /// <returns>A <see cref="MessageOfTheDay"/> that may or may not be set
        /// to display, but subsequent calls on the same build should not display.</returns>
        public MessageOfTheDay GetMotdForDisplay()
        {
            if (!this.shouldDisplay)
            {
                return MessageOfTheDay.Hidden;
            }
            else
            {
                // Do not display again for this session.
                this.shouldDisplay = false;

                // Short-circuit if another provider (maybe another device) has already shown this version.
                if (!ShouldDisplayBasedOnSettings())
                {
                    return MessageOfTheDay.Hidden;
                }

                // Do not display again for future sessions (with this version).
                this.settingsProvider.Set(SettingsKey, this.appVersion);

                return new MessageOfTheDay(this.motdTitle, this.motdBody, this.motdDismissText);
            }
        }

        /// <summary>
        /// Helper to calculate whether we should display the MOTD based on the version, compared to app version
        /// and what was last shown to the user.
        /// </summary>
        /// <returns>Whether the MOTD should display.</returns>
        private bool ShouldDisplayBasedOnSettings()
        {
            string lastShown = this.settingsProvider.Get<string>(SettingsKey, null);

            // Only show the MOTD if the version is correct and we haven't shown it yet on this build.
            return (this.appVersion != lastShown) && (this.appVersion == this.motdVersion);
        }
    }
}

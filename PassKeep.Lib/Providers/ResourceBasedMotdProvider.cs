using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using Windows.ApplicationModel;

namespace PassKeep.Lib.Providers
{
    /// <summary>
    /// A provider that provides message-of-the-day information based on an <see cref="IResourceProvider"/>
    /// and settings interfaces. The MOTD will be displayed if one has not been shown for the current version,
    /// and settings specify that it is permitted by the user.
    /// </summary>
    public sealed class ResourceBasedMotdProvider : BindableBase, IMotdProvider
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

        private readonly IResourceProvider resourceProvider;
        private readonly ISettingsProvider settingsProvider;
        private readonly string appVersion;

        private bool _shouldDisplay;

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
            this.settingsProvider = settingsProvider;

            if (appSettings.EnableMotd)
            {
                string motdVersion = resources.GetString(VersionResouceKey);

                // Get the current app version and compare to what settings says was the last shown version.
                PackageVersion pkgVersion = Package.Current.Id.Version;
                this.appVersion = $"{pkgVersion.Major}.{pkgVersion.Minor}.{pkgVersion.Revision}";
                string lastShown = this.settingsProvider.Get<string>(SettingsKey, null);

                // Only show the MOTD if the version is correct and we haven't shown it yet on this build.
                this.ShouldDisplay = (this.appVersion != lastShown) && (this.appVersion == motdVersion);
            }
            else
            {
                this.ShouldDisplay = false;
            }
        }

        /// <summary>
        /// Evaluates whether the message-of-the-day should be displayed.
        /// </summary>
        public bool ShouldDisplay
        {
            get { return this._shouldDisplay; }
            set { TrySetProperty(ref this._shouldDisplay, value); }
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

        /// <summary>
        /// Flags this MOTD as "displayed" so it will not display again in the
        /// current or future sessions.
        /// </summary>
        public void MarkAsDisplayed()
        {
            Dbg.Assert(this.ShouldDisplay);
            if (this.ShouldDisplay)
            {
                // Do not display again for this session.
                this.ShouldDisplay = false;

                // Do not display again for future sessions (with this version).
                this.settingsProvider.Set(SettingsKey, this.appVersion);
            }
        }
    }
}

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Providers;
using PassKeep.Lib.Services;
using PassKeep.Tests.Mocks;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;

namespace PassKeep.Tests
{
    /// <summary>
    /// Tests for message-of-the-day logic for <see cref="ResourceBasedMotdProvider"/>.
    /// </summary>
    [TestClass]
    public sealed class MotdTests : TestClassBase
    {
        private ISettingsProvider settingsProvider;
        private IAppSettingsService settingsService;
        private IResourceProvider resourceProvider;
        private ResourceBasedMotdProvider providerUnderTest;

        public override TestContext TestContext
        {
            get; set;
        }

        [TestInitialize]
        public void Init()
        {
            this.settingsProvider = new InMemorySettingsProvider();
            this.settingsService = new AppSettingsService(this.settingsProvider);

            PackageVersion appVersion = Package.Current.Id.Version;
            string resourceVersion = $"{appVersion.Major}.{appVersion.Minor}.{appVersion.Revision}";

            IDictionary<string, string> resourceValues = new Dictionary<string, string>
            {
                { ResourceBasedMotdProvider.VersionResouceKey, resourceVersion }
            };

            SettingsConfigurationAttribute settingsConfiguration = GetTestAttribute<SettingsConfigurationAttribute>();
            if (settingsConfiguration != null)
            {
                this.settingsService.EnableMotd = settingsConfiguration.EnableMotd;
                if(!String.IsNullOrEmpty(settingsConfiguration.MotdResourceVersion))
                {
                    resourceValues[ResourceBasedMotdProvider.VersionResouceKey] =
                        settingsConfiguration.MotdResourceVersion;
                }
            }

            this.resourceProvider = new MockResourceProvider(resourceValues);

            this.providerUnderTest = new ResourceBasedMotdProvider(
                this.resourceProvider,
                this.settingsProvider,
                this.settingsService
            );
        }

        /// <summary>
        /// Default display settings for MOTD should be as expected.
        /// </summary>
        [TestMethod]
        public void MotdDisplayDefaults()
        {
            Assert.IsTrue(this.settingsService.EnableMotd, "MOTD display should be enabled by default");
            
            Assert.IsNotNull(
                this.settingsProvider.Get<string>(ResourceBasedMotdProvider.SettingsKey, null),
                "The settings provider should have a MOTD display key set after construction"
            );

            Assert.IsTrue(this.providerUnderTest.ShouldDisplay, "MOTD should display by default according to the provider");
        }

        /// <summary>
        /// Constructing a second MOTD provider around the same settings provider/service should not display.
        /// </summary>
        /// <remarks>
        /// This simulates launching the app a second time.
        /// </remarks>
        [TestMethod]
        public void SubsequentMotdsShouldNotDisplay()
        {
            IMotdProvider laterProvider = new ResourceBasedMotdProvider(
                this.resourceProvider,
                this.settingsProvider,
                this.settingsService
            );

            Assert.IsFalse(laterProvider.ShouldDisplay, "A second provider for the same settings should not display");
        }

        /// <summary>
        /// Displaying MOTD in the settings should override the default display case.
        /// </summary>
        [TestMethod, SettingsConfiguration(EnableMotd = false)]
        public void SettingsOverrideMotdDisplay()
        {
            Assert.IsFalse(this.settingsService.EnableMotd, "Settings should be appropriately configured for this test");
            Assert.IsFalse(this.providerUnderTest.ShouldDisplay, "Settings should override the default MOTD display");
        }

        /// <summary>
        /// The MOTD does not display if it is outdated.
        /// </summary>
        [TestMethod, SettingsConfiguration(MotdResourceVersion = "0.0.0")]
        public void MotdOnlyDisplaysForCurrentVersion()
        {
            Assert.IsTrue(this.settingsService.EnableMotd);
            Assert.IsFalse(this.providerUnderTest.ShouldDisplay, "MOTD should only display for the current version");
        }

        /// <summary>
        /// An attribute used to configure the settings repository for a test.
        /// </summary>
        private sealed class SettingsConfigurationAttribute : Attribute
        {
            /// <summary>
            /// Initializes the attribute with default values.
            /// </summary>
            public SettingsConfigurationAttribute()
            {
                this.EnableMotd = true;
                this.MotdResourceVersion = null;
            }

            /// <summary>
            /// Whether to enable MOTD display in settings.
            /// </summary>
            public bool EnableMotd
            {
                get;
                set;
            }

            /// <summary>
            /// The version string to use for the MOTD resource.
            /// </summary>
            public string MotdResourceVersion
            {
                get;
                set;
            }
        }
    }
}

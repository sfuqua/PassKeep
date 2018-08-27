using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Models;
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
        /// Tests for <see cref="MessageOfTheDay"/>.
        /// </summary>
        [TestMethod]
        public void MessageOfTheDayClass()
        {
            // Validate the static MessageOfTheDay.Hidden member
            Assert.IsNotNull(MessageOfTheDay.Hidden);
            Assert.IsFalse(MessageOfTheDay.Hidden.ShouldDisplay);

            // Validate a new instance
            MessageOfTheDay instance = new MockMotdProvider().GetMotdForDisplay();
            Assert.IsTrue(instance.ShouldDisplay);
            Assert.AreEqual(MockMotdProvider.TitleText, instance.Title);
            Assert.AreEqual(MockMotdProvider.BodyText, instance.Body);
            Assert.AreEqual(MockMotdProvider.DismissText, instance.DismissText);
        }

        /// <summary>
        /// Default display settings for MOTD should be as expected.
        /// </summary>
        [TestMethod]
        public void MotdDisplayWorkflow()
        {
            Assert.IsTrue(this.settingsService.EnableMotd, "MOTD display should be enabled by default");
            
            Assert.IsNull(
                this.settingsProvider.Get<string>(ResourceBasedMotdProvider.SettingsKey, null),
                "The settings provider should not have a MOTD display key before display"
            );

            Assert.IsTrue(this.providerUnderTest.GetMotdForDisplay().ShouldDisplay, "MOTD should display by default according to the provider");

            Assert.IsNotNull(
                this.settingsProvider.Get<string>(ResourceBasedMotdProvider.SettingsKey, null),
                "The settings provider should have a MOTD display key set after MOTD is requested"
            );

            Assert.IsFalse(this.providerUnderTest.GetMotdForDisplay().ShouldDisplay, "Subsequent MOTDs should not display");
        }

        /// <summary>
        /// Constructing a second MOTD provider around the same settings provider/service should be fine - whichever
        /// one displays first will win.
        /// </summary
        [TestMethod]
        public void MultipleMotdProviders()
        {
            IMotdProvider laterProvider = new ResourceBasedMotdProvider(
                this.resourceProvider,
                this.settingsProvider,
                this.settingsService
            );

            Assert.IsTrue(laterProvider.GetMotdForDisplay().ShouldDisplay, "A second provider for the same settings should display as long as it is first");
            Assert.IsFalse(laterProvider.GetMotdForDisplay().ShouldDisplay, "The second provider should only display once");
            Assert.IsFalse(this.providerUnderTest.GetMotdForDisplay().ShouldDisplay, "The first provider should not display after the second has");
        }

        /// <summary>
        /// Displaying MOTD in the settings should override the default display case.
        /// </summary>
        [TestMethod, SettingsConfiguration(EnableMotd = false)]
        public void SettingsOverrideMotdDisplay()
        {
            Assert.IsFalse(this.settingsService.EnableMotd, "Settings should be appropriately configured for this test");
            Assert.IsFalse(this.providerUnderTest.GetMotdForDisplay().ShouldDisplay, "Settings should override the default MOTD display");
        }

        /// <summary>
        /// The MOTD does not display if it is outdated.
        /// </summary>
        [TestMethod, SettingsConfiguration(MotdResourceVersion = "0.0.0")]
        public void MotdOnlyDisplaysForCurrentVersion()
        {
            Assert.IsTrue(this.settingsService.EnableMotd);
            Assert.IsFalse(this.providerUnderTest.GetMotdForDisplay().ShouldDisplay, "MOTD should only display for the current version");
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
                EnableMotd = true;
                MotdResourceVersion = null;
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

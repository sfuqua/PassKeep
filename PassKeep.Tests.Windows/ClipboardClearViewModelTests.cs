using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Providers;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace PassKeep.KeePassTests
{
    [TestClass]
    public class ClipboardClearViewModelTests : TestClassBase
    {
        private const bool DefaultClearEnabled = true;
        private const uint DefaultClearTime = 1;

        private IAppSettingsService settingsService;
        private IClipboardClearTimerViewModel viewModel;

        [TestInitialize]
        public void Initialize()
        {
            this.settingsService = new AppSettingsService(new InMemorySettingsProvider())
            {
                EnableClipboardTimer = DefaultClearEnabled,
                ClearClipboardOnTimer = DefaultClearTime
            };

            this.viewModel = new SettingsBasedClipboardClearViewModel(this.settingsService);
        }

        [TestMethod]
        public void ClipboardClearViewModel_DefaultValues()
        {
            Assert.AreEqual(DefaultClearEnabled, this.viewModel.PasswordClearEnabled, "PasswordClearEnabled should match settings");
            Assert.AreEqual(DefaultClearEnabled, this.viewModel.UserNameClearEnabled, "UserNameClearEnabled should match settings");
            Assert.AreEqual(0, this.viewModel.PasswordTimeRemaining, "Password timer should be empty by default");
            Assert.AreEqual(0, this.viewModel.UserNameTimeRemaining, "UserName timer should be empty by default");
        }

        [TestMethod, Timeout(1000)]
        public async Task ClipboardClearViewModel_DisableViaSettings()
        {
            TaskCompletionSource<object> usernameTcs = new TaskCompletionSource<object>();
            TaskCompletionSource<object> passwordTcs = new TaskCompletionSource<object>();

            this.viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "PasswordClearEnabled")
                {
                    passwordTcs.SetResult(null);
                }
                else if (e.PropertyName == "UserNameClearEnabled")
                {
                    usernameTcs.SetResult(null);
                }
            };

            this.settingsService.EnableClipboardTimer = !DefaultClearEnabled;
            await Task.WhenAll(usernameTcs.Task, passwordTcs.Task);

            Assert.AreEqual(!DefaultClearEnabled, this.viewModel.PasswordClearEnabled, "PasswordClearEnabled should match settings");
            Assert.AreEqual(!DefaultClearEnabled, this.viewModel.UserNameClearEnabled, "UserNameClearEnabled should match settings");
        }
    }
}

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Providers;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using PassKeep.Tests.Mocks;
using SariphLib.Testing;
using System.Threading.Tasks;

namespace PassKeep.Tests
{
    [TestClass]
    public class ClipboardClearViewModelTests : TestClassBase
    {
        private const bool DefaultClearEnabled = true;
        private const uint DefaultClearTime = 1;

        private IAppSettingsService settingsService;
        private IClipboardClearTimerViewModel viewModel;

        public override TestContext TestContext
        {
            get;
            set;
        }

        [TestInitialize]
        public async Task Initialize()
        {
            this.settingsService = new AppSettingsService(new InMemorySettingsProvider())
            {
                EnableClipboardTimer = DefaultClearEnabled,
                ClearClipboardOnTimer = DefaultClearTime
            };

            this.viewModel = new SettingsBasedClipboardClearViewModel(new ThreadPoolTimerFactory(), new MockSyncContext(), this.settingsService);
            await this.viewModel.ActivateAsync();
        }

        [TestMethod]
        public void ClipboardClearViewModel_DefaultValues()
        {
            Assert.AreEqual(DefaultClearEnabled, this.viewModel.PasswordClearEnabled, "PasswordClearEnabled should match settings");
            Assert.AreEqual(DefaultClearEnabled, this.viewModel.UserNameClearEnabled, "UserNameClearEnabled should match settings");
            Assert.AreEqual(0, this.viewModel.NormalizedPasswordTimeRemaining, "Password timer should be empty by default");
            Assert.AreEqual(0, this.viewModel.NormalizedUserNameTimeRemaining, "UserName timer should be empty by default");
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

        [TestMethod, Timeout(5000)]
        public async Task ClipboardClearViewModel_UserNameTimerCompletes()
        {
            this.settingsService.EnableClipboardTimer = true;
            this.settingsService.ClearClipboardOnTimer = 2;
            this.viewModel.StartTimer(ClipboardOperationType.UserName);

            Assert.AreEqual(2, this.viewModel.TimeRemainingInSeconds);
            Assert.AreEqual(1, this.viewModel.NormalizedTimeRemaining);

            bool eventFired = false;
            this.viewModel.TimerComplete += (s, e) =>
            {
                Assert.AreEqual(ClipboardOperationType.UserName, e.TimerType);
                eventFired = true;
            };

            await AwaitableTimeout(2200);
            Assert.IsTrue(eventFired);
            Assert.AreEqual(0, this.viewModel.TimeRemainingInSeconds);
            Assert.AreEqual(0, this.viewModel.NormalizedTimeRemaining);
        }

        [TestMethod, Timeout(5000)]
        public async Task ClipboardClearViewModel_PasswordTimerCompletes()
        {
            this.settingsService.EnableClipboardTimer = true;
            this.settingsService.ClearClipboardOnTimer = 2;
            this.viewModel.StartTimer(ClipboardOperationType.Password);

            Assert.AreEqual(2, this.viewModel.TimeRemainingInSeconds);
            Assert.AreEqual(1, this.viewModel.NormalizedTimeRemaining);

            bool eventFired = false;
            this.viewModel.TimerComplete += (s, e) =>
            {
                Assert.AreEqual(ClipboardOperationType.Password, e.TimerType);
                eventFired = true;
            };

            await AwaitableTimeout(2200);
            Assert.IsTrue(eventFired);
            Assert.AreEqual(0, this.viewModel.TimeRemainingInSeconds);
            Assert.AreEqual(0, this.viewModel.NormalizedTimeRemaining);
        }

        [TestMethod, Timeout(5000)]
        public async Task ClipboardClearViewModel_TimerUsesProvidedValue()
        {
            this.settingsService.EnableClipboardTimer = true;
            this.settingsService.ClearClipboardOnTimer = 10;
            this.viewModel.StartTimer(ClipboardOperationType.UserName);

            bool eventFired = false;
            this.viewModel.TimerComplete += (s, e) =>
            {
                Assert.AreEqual(ClipboardOperationType.UserName, e.TimerType);
                eventFired = true;
            };

            await AwaitableTimeout(3000);
            Assert.IsFalse(eventFired);
        }

        [TestMethod, Timeout(5000)]
        public async Task ClipboardClearViewModel_TimersOverrideEachOther()
        {
            this.settingsService.EnableClipboardTimer = true;
            this.settingsService.ClearClipboardOnTimer = 1;
            
            this.viewModel.StartTimer(ClipboardOperationType.UserName);
            await AwaitableTimeout(500);
            this.viewModel.StartTimer(ClipboardOperationType.Password);

            bool eventFired = false;
            this.viewModel.TimerComplete += (s, e) =>
            {
                Assert.AreEqual(ClipboardOperationType.Password, e.TimerType);
                eventFired = true;
            };

            await AwaitableTimeout(1500);
            Assert.IsTrue(eventFired);
        }

        [TestMethod, Timeout(5000)]
        public async Task ClipboardClearViewModel_UserNameTimerReflectsReality()
        {
            this.settingsService.EnableClipboardTimer = true;
            this.settingsService.ClearClipboardOnTimer = 1;
            this.viewModel.StartTimer(ClipboardOperationType.UserName);

            int usernameChangeCount = 0;
            int normalizedUsernameChangeCount = 0;
            int changeCount = 0;
            int normalizedChangeCount = 0;
            this.viewModel.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case "NormalizedUserNameTimeRemaining":
                        normalizedUsernameChangeCount++;
                        break;
                    case "NormalizedPasswordTimeRemaining":
                        Assert.Fail();
                        break;
                    case "NormalizedTimeRemaining":
                        normalizedChangeCount++;
                        break;
                    case "UserNameTimeRemainingInSeconds":
                        usernameChangeCount++;
                        break;
                    case "PasswordTimeRemainingInSeconds":
                        Assert.Fail();
                        break;
                    case "TimeRemainingInSeconds":
                        changeCount++;
                        break;
                }

                Assert.AreEqual(this.viewModel.NormalizedTimeRemaining, this.viewModel.NormalizedUserNameTimeRemaining);
                Assert.AreEqual(this.viewModel.TimeRemainingInSeconds, this.viewModel.UserNameTimeRemainingInSeconds);
            };

            await AwaitableTimeout(1500);
            Assert.AreEqual(changeCount, usernameChangeCount);
            Assert.AreEqual(normalizedChangeCount, normalizedUsernameChangeCount);
            Assert.AreEqual(changeCount, normalizedChangeCount);
            Assert.IsTrue(changeCount > 0);
            Assert.IsTrue(normalizedChangeCount > 0);
        }

        [TestMethod, Timeout(5000)]
        public async Task ClipboardClearViewModel_PasswordTimerReflectsReality()
        {
            this.settingsService.EnableClipboardTimer = true;
            this.settingsService.ClearClipboardOnTimer = 1;
            this.viewModel.StartTimer(ClipboardOperationType.Password);

            int passwordChangeCount = 0;
            int normalizedPasswordChangeCount = 0;
            int changeCount = 0;
            int normalizedChangeCount = 0;
            this.viewModel.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case "NormalizedUserNameTimeRemaining":
                        Assert.Fail();
                        break;
                    case "NormalizedPasswordTimeRemaining":
                        normalizedPasswordChangeCount++;
                        break;
                    case "NormalizedTimeRemaining":
                        normalizedChangeCount++;
                        break;
                    case "UserNameTimeRemainingInSeconds":
                        Assert.Fail();
                        break;
                    case "PasswordTimeRemainingInSeconds":
                        passwordChangeCount++;
                        break;
                    case "TimeRemainingInSeconds":
                        changeCount++;
                        break;
                }

                Assert.AreEqual(this.viewModel.NormalizedTimeRemaining, this.viewModel.NormalizedPasswordTimeRemaining);
                Assert.AreEqual(this.viewModel.TimeRemainingInSeconds, this.viewModel.PasswordTimeRemainingInSeconds);
            };

            await AwaitableTimeout(1500);
            Assert.AreEqual(changeCount, passwordChangeCount);
            Assert.AreEqual(normalizedChangeCount, normalizedPasswordChangeCount);
            Assert.AreEqual(changeCount, normalizedChangeCount);
            Assert.IsTrue(changeCount > 0);
            Assert.IsTrue(normalizedChangeCount > 0);
        }

        [TestMethod]
        public async Task ClipboardClearViewModel_SetWhenDisabled()
        {
            this.settingsService.EnableClipboardTimer = false;

            bool timerRunning = false;
            this.viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(this.viewModel.TimeRemainingInSeconds))
                {
                    timerRunning = true;
                }
            };
            
            this.viewModel.StartTimer(ClipboardOperationType.UserName);
            await Task.Delay(1000);

            Assert.IsFalse(timerRunning, "Timer should not be ticking when it is disabled");
        }

        [TestMethod]
        public async Task ClipboardClearViewModel_DisableInProgress()
        {
            this.settingsService.EnableClipboardTimer = true;
            this.settingsService.ClearClipboardOnTimer = 1;

            TaskCompletionSource<bool> timerTickTcs = new TaskCompletionSource<bool>();
            int tickCount = 0;
            this.viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(this.viewModel.TimeRemainingInSeconds))
                {
                    tickCount++;
                    if (tickCount == 1)
                    {
                        timerTickTcs.SetResult(true);
                    }
                }
            };

            bool timerCompleteFired = false;
            this.viewModel.TimerComplete += (s, e) =>
            {
                timerCompleteFired = true;
            };

            this.viewModel.StartTimer(ClipboardOperationType.UserName);

            // Wait until the timer is running for sure
            await timerTickTcs.Task;

            this.settingsService.EnableClipboardTimer = false;
            int lastTickCount = tickCount;

            await Task.Delay(2000);
            Assert.AreEqual(lastTickCount, tickCount, "Timer should not have ticked since being disabled");
            Assert.IsFalse(timerCompleteFired, "Timer should not have completed after being interrupted");
            Assert.AreEqual(0, this.viewModel.TimeRemainingInSeconds, "Timer should have been zeroed on interrupt");
        }
    }
}

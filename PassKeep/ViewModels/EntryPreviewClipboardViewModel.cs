using System;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace PassKeep.ViewModels
{
    public enum ClipboardTimerType
    {
        None,
        UserName,
        Password
    }

    public class EntryPreviewClipboardViewModel : ViewModelBase
    {
        private DispatcherTimer currentTimer;
        private ClipboardTimerType currentTimerType;
        private double timerDurationInSeconds;
        private double elapsedTimeInSeconds;
        private const double TimerIntervalInSeconds = 0.1;

        public bool UserNameClearEnabled
        {
            get { return Settings.EnableClipboardTimer; }
        }

        private double _userNameTimeRemaining;
        public double UserNameTimeRemaining
        {
            get { return _userNameTimeRemaining; }
            set { SetProperty(ref _userNameTimeRemaining, value); }
        }

        public bool PasswordClearEnabled
        {
            get { return Settings.EnableClipboardTimer; }
        }

        private double _passwordTimeRemaining;
        public double PasswordTimeRemaining
        {
            get { return _passwordTimeRemaining; }
            set { SetProperty(ref _passwordTimeRemaining, value); }
        }

        /// <summary>
        /// Fired when one of the timers successfully terminates.
        /// </summary>
        public event EventHandler<ClipboardTimerCompleteEventArgs> TimerComplete;
        private void onTimerComplete()
        {
            if (TimerComplete != null)
            {
                TimerComplete(this, new ClipboardTimerCompleteEventArgs(currentTimerType));
            }
        }

        public EntryPreviewClipboardViewModel(ConfigurationViewModel appSettings)
            : base(appSettings)
        {
            UserNameTimeRemaining = 0;
            PasswordTimeRemaining = 0;
        }

        private void timerTick(object sender, object e)
        {
            Debug.Assert(currentTimerType != ClipboardTimerType.None);

            elapsedTimeInSeconds += TimerIntervalInSeconds;
            double newNormalizedValue = Math.Max(0, (timerDurationInSeconds - elapsedTimeInSeconds) / timerDurationInSeconds);

            switch (currentTimerType)
            {
                case ClipboardTimerType.UserName:
                    UserNameTimeRemaining = newNormalizedValue;
                    break;
                case ClipboardTimerType.Password:
                    PasswordTimeRemaining = newNormalizedValue;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (newNormalizedValue == 0)
            {
                currentTimer.Tick -= timerTick;
                currentTimer.Stop();
                currentTimer = null;
                onTimerComplete();
                currentTimerType = ClipboardTimerType.None;
                return;
            }
        }

        public void StartTimer(ClipboardTimerType timerType)
        {
            Debug.Assert(timerType != ClipboardTimerType.None);
            if (timerType == ClipboardTimerType.None)
            {
                throw new ArgumentException("cannot start a timer with no type", "timerType");
            }

            switch (timerType)
            {
                case ClipboardTimerType.UserName:
                    PasswordTimeRemaining = 0;
                    UserNameTimeRemaining = 1;
                    break;
                case ClipboardTimerType.Password:
                    UserNameTimeRemaining = 0;
                    PasswordTimeRemaining = 1;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (currentTimer != null)
            {
                currentTimer.Tick -= timerTick;
                currentTimer.Stop();
            }

            timerDurationInSeconds = Settings.ClearClipboardOnTimer;
            elapsedTimeInSeconds = 0;
            currentTimerType = timerType;
            currentTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(TimerIntervalInSeconds) };
            currentTimer.Tick += timerTick;
            currentTimer.Start();
        }
    }

    public class ClipboardTimerCompleteEventArgs : EventArgs
    {
        public ClipboardTimerType TimerType
        {
            get;
            set;
        }

        public bool Handled
        {
            get;
            set;
        }

        public ClipboardTimerCompleteEventArgs(ClipboardTimerType timerType)
        {
            TimerType = timerType;
            Handled = false;
        }
    }
}

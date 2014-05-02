using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Eventing;
using SariphLib.Mvvm;
using System;
using System.ComponentModel;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel for handling clipboard clear timers that interacts with the app's settings.
    /// </summary>
    public class SettingsBasedClipboardClearViewModel : BindableBase, IClipboardClearTimerViewModel
    {
        private const double TimerIntervalInSeconds = 0.1;

        private IAppSettingsService settingsService;
        private ClipboardTimerType currentTimerType;
        private DispatcherTimer currentTimer;
        private double durationOfCurrentTimerInSeconds;
        private double elapsedTimeInSeconds;

        private double _userNameTimeRemaining;
        private double _passwordTimeRemaining;

        /// <summary>
        /// Initializes the ViewModel.
        /// </summary>
        /// <param name="settingsService">A service for retrieving app settings.</param>
        public SettingsBasedClipboardClearViewModel(IAppSettingsService settingsService)
        {
            this.settingsService = settingsService;
            this.currentTimerType = ClipboardTimerType.None;
            this.durationOfCurrentTimerInSeconds = 0;
            this.elapsedTimeInSeconds = 0;
            this.UserNameTimeRemaining = 0;
            this.PasswordTimeRemaining = 0;

            this.settingsService.PropertyChanged +=
                new WeakEventHandler<PropertyChangedEventArgs>(OnSettingsServicePropertyChanged).Handler;
        }

        /// <summary>
        /// Fired when a timer successfully terminates.
        /// </summary>
        public event EventHandler<ClipboardTimerCompleteEventArgs> TimerComplete;

        /// <summary>
        /// Fires the TimerComplete event.
        /// </summary>
        private void FireTimerComplete()
        {
            if (TimerComplete != null)
            {
                TimerComplete(this, new ClipboardTimerCompleteEventArgs(this.currentTimerType));
            }
        }

        /// <summary>
        /// Whether the ViewModel supports clearing the clipboard for a username copy.
        /// </summary>
        public bool UserNameClearEnabled
        {
            get
            {
                return this.settingsService.EnableClipboardTimer;
            }
        }

        /// <summary>
        /// The amount of time remaining for the current username clear timer (0 to 1).
        /// </summary>
        public double UserNameTimeRemaining
        {
            get { return this._userNameTimeRemaining; }
            private set { TrySetProperty(ref this._userNameTimeRemaining, value);  }
        }

        /// <summary>
        /// Whether the ViewModel supports clearing the clipboard for a password copy.
        /// </summary>
        public bool PasswordClearEnabled
        {
            get
            {
                return this.settingsService.EnableClipboardTimer;
            }
        }

        /// <summary>
        /// The amount of time remaining for the current password clear timer (0 to 1).
        /// </summary>
        public double PasswordTimeRemaining
        {
            get { return this._passwordTimeRemaining; }
            private set { TrySetProperty(ref this._passwordTimeRemaining, value); }
        }

        /// <summary>
        /// Starts the clipboard clear timer, resetting any existing timers.
        /// </summary>
        /// <param name="timerType">The type of clipboard timer being started.</param>
        public void StartTimer(ClipboardTimerType timerType)
        {
            if (timerType == ClipboardTimerType.None)
            {
                throw new ArgumentException("cannot start a timer with no type", "timerType");
            }

            switch (timerType)
            {
                case ClipboardTimerType.UserName:
                    this.PasswordTimeRemaining = 0;
                    this.UserNameTimeRemaining = 1;
                    break;
                case ClipboardTimerType.Password:
                    this.UserNameTimeRemaining = 0;
                    this.PasswordTimeRemaining = 1;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            // If we have an existing timer, kill it.
            if (this.currentTimer != null)
            {
                this.currentTimer.Tick -= OnTimerTick;
                this.currentTimer.Stop();
            }
            
            // Set up the new timer.
            this.durationOfCurrentTimerInSeconds = this.settingsService.ClearClipboardOnTimer;
            this.elapsedTimeInSeconds = 0;
            this.currentTimerType = timerType;
            this.currentTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(TimerIntervalInSeconds) };
            this.currentTimer.Tick += OnTimerTick;
            this.currentTimer.Start();
        }

        /// <summary>
        /// Handles changes to properties on the settings service.
        /// In particular, whether the clipboard timer is enabled.
        /// </summary>
        /// <param name="sender">The settings service whose property changed.</param>
        /// <param name="e">Event args for the property change.</param>
        private void OnSettingsServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "EnableClipboardTimer")
            {
                OnPropertyChanged("UserNameClearEnabled");
                OnPropertyChanged("PasswordClearEnabled");
            }
        }

        /// <summary>
        /// Handles the Tick event for internal timer.
        /// </summary>
        /// <param name="sender">The ticking timer.</param>
        /// <param name="e">The argument for the event.</param>
        private void OnTimerTick(object sender, object e)
        {
            DispatcherTimer currentTimer = (DispatcherTimer)sender;
            Debug.Assert(currentTimer.Interval.Seconds == TimerIntervalInSeconds);

            // Increment the elapsed time of the timer
            this.elapsedTimeInSeconds += TimerIntervalInSeconds;

            // Normalize the value (we count down from 0 to 1)
            double newNormalizedValue = Math.Max(
                0,
                (this.durationOfCurrentTimerInSeconds - this.elapsedTimeInSeconds) / this.durationOfCurrentTimerInSeconds
            );

            // Update the appropriate property
            switch (currentTimerType)
            {
                case ClipboardTimerType.UserName:
                    this.UserNameTimeRemaining = newNormalizedValue;
                    break;
                case ClipboardTimerType.Password:
                    this.PasswordTimeRemaining = newNormalizedValue;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            // Check to see if the timer is done.
            if (newNormalizedValue == 0)
            {
                this.currentTimer.Tick -= OnTimerTick;
                this.currentTimer.Stop();
                this.currentTimer = null;
                FireTimerComplete();
                this.currentTimerType = ClipboardTimerType.None;
            }
        }
    }
}

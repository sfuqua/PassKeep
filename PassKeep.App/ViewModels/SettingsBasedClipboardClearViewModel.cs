using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Eventing;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using System.ComponentModel;
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
        private ClipboardOperationType currentTimerType;
        private ITimer currentTimer;
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
            this.currentTimerType = ClipboardOperationType.None;
            this.durationOfCurrentTimerInSeconds = 0;
            this.elapsedTimeInSeconds = 0;
            this.NormalizedUserNameTimeRemaining = 0;
            this.NormalizedPasswordTimeRemaining = 0;

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
        public double NormalizedUserNameTimeRemaining
        {
            get { return this._userNameTimeRemaining; }
            private set
            {
                if (TrySetProperty(ref this._userNameTimeRemaining, value))
                {
                    OnPropertyChanged("UserNameTimeRemainingInSeconds");
                    if (this.currentTimerType == ClipboardOperationType.UserName)
                    {
                        OnPropertyChanged("NormalizedTimeRemaining");
                        OnPropertyChanged("TimeRemainingInSeconds");
                    }
                }
            }
        }
         
        /// <summary>
        /// Gets the time remaining in seconds for the username clear timer.
        /// </summary>
        public double UserNameTimeRemainingInSeconds
        {
            get
            {
                if (this.currentTimerType != ClipboardOperationType.UserName)
                {
                    return 0;
                }

                return this.durationOfCurrentTimerInSeconds - this.elapsedTimeInSeconds;
            }
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
        public double NormalizedPasswordTimeRemaining
        {
            get { return this._passwordTimeRemaining; }
            private set
            {
                if (TrySetProperty(ref this._passwordTimeRemaining, value))
                {
                    OnPropertyChanged("PasswordTimeRemainingInSeconds");
                    if (this.currentTimerType == ClipboardOperationType.Password)
                    {
                        OnPropertyChanged("NormalizedTimeRemaining");
                        OnPropertyChanged("TimeRemainingInSeconds");
                    }
                }
            }
        }

        /// <summary>
        /// Gets the time remaining in seconds for the password clear timer.
        /// </summary>
        public double PasswordTimeRemainingInSeconds
        {
            get
            {
                if (this.currentTimerType != ClipboardOperationType.Password)
                {
                    return 0;
                }

                return this.durationOfCurrentTimerInSeconds - this.elapsedTimeInSeconds;
            }
        }

        /// <summary>
        /// Gets the normalized remaining time [0, 1] for the current timer.
        /// </summary>
        public double NormalizedTimeRemaining
        {
            get
            {
                switch (this.currentTimerType)
                {
                    case ClipboardOperationType.UserName:
                        return this.NormalizedUserNameTimeRemaining;
                    case ClipboardOperationType.Password:
                        return this.NormalizedPasswordTimeRemaining;
                    default:
                        Dbg.Assert(this.currentTimerType == ClipboardOperationType.None);
                        return 0;
                }
            }
        }

        /// <summary>
        /// Gets the time remaining in seconds for the current timer.
        /// </summary>
        public double TimeRemainingInSeconds
        {
            get
            {
                switch (this.currentTimerType)
                {
                    case ClipboardOperationType.UserName:
                        return this.UserNameTimeRemainingInSeconds;
                    case ClipboardOperationType.Password:
                        return this.PasswordTimeRemainingInSeconds;
                    default:
                        Dbg.Assert(this.currentTimerType == ClipboardOperationType.None);
                        return 0;
                }
            }
        }

        /// <summary>
        /// Starts the clipboard clear timer, resetting any existing timers.
        /// </summary>
        /// <typeparam name="TTimer">The concrete type of timer to start.</typeparam>
        /// <param name="timerType">The type of clipboard timer being started.</param>
        public void StartTimer<TTimer>(ClipboardOperationType timerType)
            where TTimer : ITimer, new()
        {
            if (timerType == ClipboardOperationType.None)
            {
                throw new ArgumentException("cannot start a timer with no type", "timerType");
            }

            // If we have an existing timer, kill it.
            if (this.currentTimer != null)
            {
                this.currentTimer.Tick -= OnTimerTick;
                this.currentTimer.Stop();
            }

            // Set up the new timer.
            this.currentTimerType = timerType;
            switch (timerType)
            {
                case ClipboardOperationType.UserName:
                    this.NormalizedPasswordTimeRemaining = 0;
                    this.NormalizedUserNameTimeRemaining = 1;
                    break;
                case ClipboardOperationType.Password:
                    this.NormalizedUserNameTimeRemaining = 0;
                    this.NormalizedPasswordTimeRemaining = 1;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            this.durationOfCurrentTimerInSeconds = this.settingsService.ClearClipboardOnTimer;
            this.elapsedTimeInSeconds = 0;
            this.currentTimer = new TTimer { Interval = TimeSpan.FromSeconds(TimerIntervalInSeconds) };
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
            ITimer currentTimer = (ITimer)sender;
            Dbg.Assert(currentTimer.Interval == TimeSpan.FromSeconds(TimerIntervalInSeconds));

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
                case ClipboardOperationType.UserName:
                    this.NormalizedUserNameTimeRemaining = newNormalizedValue;
                    break;
                case ClipboardOperationType.Password:
                    this.NormalizedPasswordTimeRemaining = newNormalizedValue;
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
                this.currentTimerType = ClipboardOperationType.None;
            }
        }
    }
}

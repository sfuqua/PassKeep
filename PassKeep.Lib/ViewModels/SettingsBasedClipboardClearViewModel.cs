// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Diagnostics;
using SariphLib.Mvvm;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel for handling clipboard clear timers that interacts with the app's settings.
    /// </summary>
    public class SettingsBasedClipboardClearViewModel : AbstractViewModel, IClipboardClearTimerViewModel
    {
        private const double TimerIntervalInSeconds = 0.1;

        private ITimerFactory timerFactory;
        private ISyncContext syncContext;
        private IAppSettingsService settingsService;
        private ClipboardOperationType currentTimerType;
        private ITimer currentTimer;
        private double durationOfCurrentTimerInSeconds;
        private double elapsedTimeInSeconds;

        private DateTime? suspendTime;

        private double _userNameTimeRemaining;
        private double _passwordTimeRemaining;
        private double _otherTimeRemaining;

        /// <summary>
        /// Initializes the ViewModel.
        /// </summary>
        /// <param name="settingsService">A service for retrieving app settings.</param>
        public SettingsBasedClipboardClearViewModel(
            ITimerFactory timerFactory,
            ISyncContext syncContext,
            IAppSettingsService settingsService
        )
        {
            this.timerFactory = timerFactory;
            this.syncContext = syncContext;
            this.settingsService = settingsService;
            this.currentTimerType = ClipboardOperationType.None;
            this.durationOfCurrentTimerInSeconds = 0;
            this.elapsedTimeInSeconds = 0;
            NormalizedUserNameTimeRemaining = 0;
            NormalizedPasswordTimeRemaining = 0;
            NormalizedOtherTimeRemaining = 0;
        }

        public override async Task ActivateAsync()
        {
            await base.ActivateAsync();
            this.settingsService.PropertyChanged += OnSettingsServicePropertyChanged;
        }

        public override async Task SuspendAsync()
        {
            await base.SuspendAsync();
            this.settingsService.PropertyChanged -= OnSettingsServicePropertyChanged;
        }

        /// <summary>
        /// Records the suspension time to recalculate timeouts later.
        /// </summary>
        public override void HandleAppSuspend()
        {
            this.suspendTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Adjusts the timer based on how long we were suspended.
        /// </summary>
        public override void HandleAppResume()
        {
            if (this.suspendTime == null || !this.settingsService.EnableClipboardTimer)
            {
                return;
            }

            TimeSpan timeSuspended = DateTime.UtcNow.Subtract(this.suspendTime.Value);
            this.suspendTime = null;

            IncrementElapsedTime(timeSuspended.TotalSeconds);
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
                    OnPropertyChanged(nameof(UserNameTimeRemainingInSeconds));
                    if (this.currentTimerType == ClipboardOperationType.UserName)
                    {
                        OnPropertyChanged(nameof(NormalizedTimeRemaining));
                        OnPropertyChanged(nameof(TimeRemainingInSeconds));
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

                return Math.Max(this.durationOfCurrentTimerInSeconds - this.elapsedTimeInSeconds, 0);
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
                    OnPropertyChanged(nameof(PasswordTimeRemainingInSeconds));
                    if (this.currentTimerType == ClipboardOperationType.Password)
                    {
                        OnPropertyChanged(nameof(NormalizedTimeRemaining));
                        OnPropertyChanged(nameof(TimeRemainingInSeconds));
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

                return Math.Max(this.durationOfCurrentTimerInSeconds - this.elapsedTimeInSeconds, 0);
            }
        }

        /// <summary>
        /// Whether the ViewModel supports clearing the clipboard for a non-credential copy.
        /// </summary>
        public bool OtherClearEnabled
        {
            get
            {
                return this.settingsService.EnableClipboardTimer;
            }
        }

        /// <summary>
        /// The amount of time remaining for the current non-credential clear timer (0 to 1).
        /// </summary>
        public double NormalizedOtherTimeRemaining
        {
            get { return this._otherTimeRemaining; }
            private set
            {
                if (TrySetProperty(ref this._otherTimeRemaining, value))
                {
                    OnPropertyChanged(nameof(OtherTimeRemainingInSeconds));
                    if (this.currentTimerType == ClipboardOperationType.Other)
                    {
                        OnPropertyChanged(nameof(NormalizedTimeRemaining));
                        OnPropertyChanged(nameof(TimeRemainingInSeconds));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the time remaining in seconds for the non-credential clear timer.
        /// </summary>
        public double OtherTimeRemainingInSeconds
        {
            get
            {
                if (this.currentTimerType != ClipboardOperationType.Other)
                {
                    return 0;
                }

                return Math.Max(this.durationOfCurrentTimerInSeconds - this.elapsedTimeInSeconds, 0);
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
                        return NormalizedUserNameTimeRemaining;
                    case ClipboardOperationType.Password:
                        return NormalizedPasswordTimeRemaining;
                    case ClipboardOperationType.Other:
                        return NormalizedOtherTimeRemaining;
                    default:
                        DebugHelper.Assert(this.currentTimerType == ClipboardOperationType.None);
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
                        return UserNameTimeRemainingInSeconds;
                    case ClipboardOperationType.Password:
                        return PasswordTimeRemainingInSeconds;
                    case ClipboardOperationType.Other:
                        return OtherTimeRemainingInSeconds;
                    default:
                        DebugHelper.Assert(this.currentTimerType == ClipboardOperationType.None);
                        return 0;
                }
            }
        }

        /// <summary>
        /// Starts the clipboard clear timer, resetting any existing timers.
        /// If the specified timer is not enabled, this call is ignored.
        /// </summary>
        /// <param name="timerType">The type of clipboard timer being started.</param>
        public void StartTimer(ClipboardOperationType timerType)
        {
            if (timerType == ClipboardOperationType.None)
            {
                throw new ArgumentException("cannot start a timer with no type", nameof(timerType));
            }

            bool timerEnabled = false;
            switch(timerType)
            {
                case ClipboardOperationType.UserName:
                    timerEnabled = UserNameClearEnabled;
                    break;
                case ClipboardOperationType.Password:
                    timerEnabled = PasswordClearEnabled;
                    break;
                case ClipboardOperationType.Other:
                    timerEnabled = OtherClearEnabled;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (!timerEnabled)
            {
                return;
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
                    NormalizedUserNameTimeRemaining = 1;
                    NormalizedPasswordTimeRemaining = 0;
                    NormalizedOtherTimeRemaining = 0;
                    break;
                case ClipboardOperationType.Password:
                    NormalizedUserNameTimeRemaining = 0;
                    NormalizedPasswordTimeRemaining = 1;
                    NormalizedOtherTimeRemaining = 0;
                    break;
                case ClipboardOperationType.Other:
                    NormalizedUserNameTimeRemaining = 0;
                    NormalizedPasswordTimeRemaining = 0;
                    NormalizedOtherTimeRemaining = 1;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            this.durationOfCurrentTimerInSeconds = this.settingsService.ClearClipboardOnTimer;
            this.elapsedTimeInSeconds = 0;
            this.currentTimer = this.timerFactory.Assemble(TimeSpan.FromSeconds(TimerIntervalInSeconds));
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
            if (e.PropertyName == nameof(this.settingsService.EnableClipboardTimer))
            {
                OnPropertyChanged(nameof(UserNameClearEnabled));
                OnPropertyChanged(nameof(PasswordClearEnabled));
                OnPropertyChanged(nameof(OtherClearEnabled));

                // If the timer has been disabled, kill any existing timers.
                if (!this.settingsService.EnableClipboardTimer)
                {
                    if (this.currentTimer != null)
                    {
                        this.currentTimer.Tick -= OnTimerTick;
                        this.currentTimer.Stop();
                        this.currentTimer = null;
                        this.currentTimerType = ClipboardOperationType.None;
                        NormalizedUserNameTimeRemaining = 0;
                        NormalizedPasswordTimeRemaining = 0;
                        NormalizedOtherTimeRemaining = 0;
                        OnPropertyChanged(nameof(NormalizedTimeRemaining));
                        OnPropertyChanged(nameof(TimeRemainingInSeconds));
                    }
                }
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
            DebugHelper.Assert(currentTimer.Interval == TimeSpan.FromSeconds(TimerIntervalInSeconds));

            IncrementElapsedTime(TimerIntervalInSeconds);
        }

        /// <summary>
        /// Helper that handles incrementing the timer.
        /// </summary>
        /// <param name="timePassed">How much time to increment by.</param>
        private void IncrementElapsedTime(double timePassed)
        {
            // Increment the elapsed time of the timer
            this.elapsedTimeInSeconds += timePassed;

            // Normalize the value (we count down from 0 to 1)
            double newNormalizedValue = Math.Max(
                0,
                (this.durationOfCurrentTimerInSeconds - this.elapsedTimeInSeconds) / this.durationOfCurrentTimerInSeconds
            );

            SetNormalizedTimerValue(newNormalizedValue);
        }

        /// <summary>
        /// Helper that handles updating the appropriate timer properties.
        /// </summary>
        /// <param name="newNormalizedValue">The timer value to use.</param>
        private void SetNormalizedTimerValue(double newNormalizedValue)
        {
            // Update the appropriate property
            this.syncContext.Post(() =>
            {
                switch (currentTimerType)
                {
                    case ClipboardOperationType.UserName:
                        NormalizedUserNameTimeRemaining = newNormalizedValue;
                        break;
                    case ClipboardOperationType.Password:
                        NormalizedPasswordTimeRemaining = newNormalizedValue;
                        break;
                    case ClipboardOperationType.Other:
                        NormalizedOtherTimeRemaining = newNormalizedValue;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            );

            CheckTimerCompletion();
        }

        /// <summary>
        /// Helper to handle the case where the timer is at 0.
        /// </summary>
        private void CheckTimerCompletion()
        {
            // Check to see if the timer is done.
            if (this.currentTimer != null && NormalizedTimeRemaining == 0)
            {
                this.currentTimer.Tick -= OnTimerTick;
                this.currentTimer.Stop();
                this.currentTimer = null;
                this.syncContext.Post(FireTimerComplete);
                this.currentTimerType = ClipboardOperationType.None;
            }
        }   
    }
}

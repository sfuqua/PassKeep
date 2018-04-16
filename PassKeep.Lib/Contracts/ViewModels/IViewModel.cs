// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Diagnostics;
using System.ComponentModel;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Used to log events for debugging.
        /// </summary>
        IEventLogger Logger { get; set; }

        /// <summary>
        /// Sets up event listeners for dependent services, other repeatable initialization steps.
        /// </summary>
        Task ActivateAsync();

        /// <summary>
        /// Releases any resources acquired by <see cref="Activate"/>, until the next acquisition.
        /// </summary>
        Task SuspendAsync();

        /// <summary>
        /// Saves state when app is being suspended.
        /// </summary>
        void HandleAppSuspend();

        /// <summary>
        /// Restores state when app is resumed.
        /// </summary>
        void HandleAppResume();
    }
}

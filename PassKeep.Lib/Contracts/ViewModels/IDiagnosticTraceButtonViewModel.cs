// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Mvvm;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Represents a button that can start/stop a trace to a file.
    /// </summary>
    public interface IDiagnosticTraceButtonViewModel : IViewModel
    {
        /// <summary>
        /// Whether a trace is currently running.
        /// </summary>
        bool IsTracing { get; }

        /// <summary>
        /// The label to display to the view based on the current state.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Starts/stops a trace based on the current state.
        /// </summary>
        IAsyncCommand Command { get; }
    }
}

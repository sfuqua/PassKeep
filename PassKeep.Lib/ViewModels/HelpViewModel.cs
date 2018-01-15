// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.ViewModels;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// ViewModel used for a help panel.
    /// </summary>
    public sealed class HelpViewModel : AbstractViewModel, IHelpViewModel
    {
        private readonly IDiagnosticTraceButtonViewModel traceViewModel;

        /// <summary>
        /// Initializes the ViewModel with the given dependencies.
        /// </summary>
        /// <param name="traceViewModel"></param>
        public HelpViewModel(IDiagnosticTraceButtonViewModel traceViewModel)
        {
            this.traceViewModel = traceViewModel;
        }

        /// <summary>
        /// ViewModel used for starting/stopping traces.
        /// </summary>
        public IDiagnosticTraceButtonViewModel TraceViewModel
        {
            get { return this.traceViewModel; }
        }
    }
}

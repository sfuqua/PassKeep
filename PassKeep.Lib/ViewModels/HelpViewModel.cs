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

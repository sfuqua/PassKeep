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

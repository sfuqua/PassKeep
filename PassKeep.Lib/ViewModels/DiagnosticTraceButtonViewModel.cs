using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Lib.ViewModels
{
    public sealed class DiagnosticTraceButtonViewModel : AbstractViewModel
    {
        /// <summary>
        /// Whether a trace is currently running.
        /// </summary>
        public bool IsTracing { get; }

        /// <summary>
        /// The label to display to the view based on the current state.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Starts/stops a trace based on the current state.
        /// </summary>
        public IAsyncCommand Command { get; }
    }
}

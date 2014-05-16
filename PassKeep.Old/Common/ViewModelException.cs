using System;

namespace PassKeep.Common
{
    /// <summary>
    /// Represents a problem with loading or binding to a ViewModel.
    /// </summary>
    public class ViewModelException: Exception
    {
        public ViewModelException() { }
        public ViewModelException(string message) : base(message) { }
    }
}

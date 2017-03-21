using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.ViewModels;
using Windows.Storage;

namespace PassKeep.Framework.Messages
{
    /// <summary>
    /// A message for communicating that a new database file was opened.
    /// </summary>
    public sealed class DatabaseOpenedMessage : MessageBase
    {
        /// <summary>
        /// Constructs the message.
        /// </summary>
        /// <param name="viewModel">A ViewModel representing the opened database.</param>
        public DatabaseOpenedMessage(IDatabaseParentViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        /// <summary>
        /// A ViewModel for the opened database.
        /// </summary>
        public IDatabaseParentViewModel ViewModel
        {
            get;
            private set;
        }
    }
}

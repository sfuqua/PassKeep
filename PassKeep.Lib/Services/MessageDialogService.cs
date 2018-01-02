// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Services;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// A service responsible for prompting a user with a
    /// yes/no decision via a <see cref="MessageDialog"/>.
    /// </summary>
    public class MessageDialogService : IUserPromptingService
    {
        private readonly MessageDialog dialog;
        private readonly UICommand yesCommand;

        /// <summary>
        /// Initializes the backing dialog with the specified strings.
        /// </summary>
        /// <param name="title">The title to use for the dialog.</param>
        /// <param name="content">The content to use for the dialog.</param>rfgvb g
        /// <param name="yesLabel">The label to use for the 'yes' button.</param>
        /// <param name="noLabel">The label to use for the 'no' button.</param>
        public MessageDialogService(
            string title,
            string content,
            string yesLabel,
            string noLabel
        )
        {
            this.dialog = new MessageDialog(content, title)
            {
                Options = MessageDialogOptions.None
            };

            this.yesCommand = new UICommand(yesLabel);
            UICommand noCommand = new UICommand(noLabel);

            this.dialog.Commands.Add(this.yesCommand);
            this.dialog.Commands.Add(noCommand);
        }

        /// <summary>
        /// Asks the user a yes or no question and returns
        /// the answer asynchronously.
        /// </summary>
        /// <returns>The result of prompting the user.</returns>
        public async Task<bool> PromptYesNoAsync()
        {
            IUICommand chosenCommand = await this.dialog.ShowAsync();
            return chosenCommand == this.yesCommand;
        }

        /// <summary>
        /// Asks the user a yes or no question and returns
        /// the answer asynchronously, with string template parameters.
        /// </summary>
        /// <param name="args">Arguments to template into the content.</param>
        /// <returns>The result of prompting the user.</returns>
        public Task<bool> PromptYesNoAsync(params object[] args)
        {
            this.dialog.Content = string.Format(this.dialog.Content, args);
            return PromptYesNoAsync();
        }
    }
}

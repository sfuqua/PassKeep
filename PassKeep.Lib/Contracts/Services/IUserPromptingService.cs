using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.Services
{
    /// <summary>
    /// A service responsible for prompting a user with a
    /// yes/no decision. 
    /// </summary>
    public interface IUserPromptingService
    {
        /// <summary>
        /// Asks the user a yes or no question and returns
        /// the answer asynchronously.
        /// </summary>
        /// <returns>The result of prompting the user.</returns>
        Task<bool> PromptYesNoAsync();

        /// <summary>
        /// Asks the user a yes or no question and returns
        /// the answer asynchronously, with string template parameters.
        /// </summary>
        /// <param name="args">Arguments to template into the content.</param>
        /// <returns>The result of prompting the user.</returns>
        Task<bool> PromptYesNoAsync(params object[] args);
    }
}

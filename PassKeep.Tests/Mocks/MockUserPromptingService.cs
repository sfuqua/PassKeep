using PassKeep.Lib.Contracts.Services;
using System.Threading.Tasks;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// A configurable implementation that simulates user consent.
    /// </summary>
    public class MockUserPromptingService : IUserPromptingService
    {
        /// <summary>
        /// The value to return from <see cref="PromptYesNoAsync"/>.
        /// </summary>
        public bool Result
        {
            get;
            set;
        }

        /// <summary>
        /// Asynchronously returns <see cref="Result"/>.
        /// </summary>
        /// <returns></returns>
        public Task<bool> PromptYesNoAsync()
        {
            return Task.FromResult(Result);
        }

        /// <summary>
        /// Asynchronously returns <see cref="Result"/>.
        /// </summary>
        /// <param name="args">Unused.</param>
        /// <returns></returns>
        public Task<bool> PromptYesNoAsync(params object[] args)
        {
            return PromptYesNoAsync();
        }
    }
}

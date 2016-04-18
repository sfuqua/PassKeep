using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using System.Threading.Tasks;

namespace PassKeep.Tests.Mocks
{
    public sealed class MockCredentialStorageViewModelFactory : ISavedCredentialsViewModelFactory
    {
        /// <summary>
        /// Asynchronously returns null.
        /// </summary>
        /// <returns></returns>
        public Task<ISavedCredentialsViewModel> AssembleAsync()
        {
            return Task.FromResult<ISavedCredentialsViewModel>(null);
        }
    }
}

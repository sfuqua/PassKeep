// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.ViewModels;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// A helper for constructing activated <see cref="ISavedCredentialsViewModel"/> instances.
    /// </summary>
    /// <remarks>
    /// These ViewModels can get stale if the PasswordVault changes underneath them. This 
    /// interface should assemble new ones before they are required.
    /// </remarks>
    public interface ISavedCredentialsViewModelFactory
    {
        /// <summary>
        /// Asynchronously constructs and activates an 
        /// <see cref="ISavedCredentialsViewModel"/>.
        /// </summary>
        /// <returns>A task that completes when the ViewModel is ready for use.</returns>
        Task<ISavedCredentialsViewModel> AssembleAsync();
    }
}

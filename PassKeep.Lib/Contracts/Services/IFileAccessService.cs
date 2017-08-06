// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Files;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.Services
{
    /// <summary>
    /// A service that allows the user to pick files.
    /// </summary>
    public interface IFileAccessService
    {
        /// <summary>
        /// Allows the user to pick a file for the purpose of opening it.
        /// </summary>
        /// <returns>A task that resolves to the picked file.</returns>
        Task<ITestableFile> PickFileForOpenAsync();

        /// <summary>
        /// Allows the user to pick a file for the purposes of saving to it.
        /// </summary>
        /// <param name="defaultName">The default filename to use (with optional extension).</param>
        /// <returns>A task that resolves to the picked file.</returns>
        Task<ITestableFile> PickFileForSaveAsync(string defaultName);
    }
}

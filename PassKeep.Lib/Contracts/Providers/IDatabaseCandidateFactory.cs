// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Contracts.Models;
using SariphLib.Files;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// An interface for assembling <see cref="IDatabaseCandidate"/> instances.
    /// </summary>
    public interface IDatabaseCandidateFactory
    {
        /// <summary>
        /// Asynchronously manufactures an <see cref="IDatabaseCandidate"/>
        /// from <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to wrap with a candidate.</param>
        /// <returns>A task that resolves to an <see cref="IDatabaseCandidate"/>.</returns>
        Task<IDatabaseCandidate> AssembleAsync(ITestableFile file);
    }
}

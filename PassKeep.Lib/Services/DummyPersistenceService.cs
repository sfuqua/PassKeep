// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Mvvm;
using System.Threading.Tasks;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// A "Service" that always no-ops for document persistence.
    /// </summary>
    public class DummyPersistenceService : BindableBase, IDatabasePersistenceService
    {
        /// <summary>
        /// The dummy service can always save.
        /// </summary>
        public bool CanSave
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// False.
        /// </summary>
        public bool IsSaving { get { return false; } }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="document">The KdbxDocument to persist.</param>
        /// <returns>A Task that will evaluate to true.</returns>
        public Task<bool> Save(KdbxDocument document)
            => Task.FromResult(true);
    }
}

using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Services;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Generates services capable of saving databases.
    /// </summary>
    public interface IDatabasePersistenceServiceFactory
    {
        /// <summary>
        /// Assembles a service that can save a database.
        /// </summary>
        /// <param name="writer">Serializes databases.</param>
        /// <param name="settings">Describes how to serialize databases.</param>
        /// <param name="candidate">What to serialize.</param>
        /// <param name="canSave">Whether saving is possible.</param>
        /// <returns></returns>
        IDatabasePersistenceService Assemble(IKdbxWriter writer, IDatabaseSettingsProvider settings, IDatabaseCandidate candidate, bool canSave);
    }
}

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Diagnostics;
using SariphLib.Files;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Allows updating the credentials for a database.
    /// </summary>
    public interface IDatabaseCredentialProvider
    {
        /// <summary>
        /// Facilitates diagnostic logging.
        /// </summary>
        IEventLogger Logger { get; set; }

        /// <summary>
        /// Asynchronously updates the provided document's credentials.
        /// </summary>
        /// <param name="document">The document to update.</param>
        /// <param name="databaseFile">A token used to locate the document in credential storage.</param>
        /// <param name="tokens">Security tokens to use for the new credential.</param>
        /// <returns>A task that completes when the update is finished.</returns>
        Task UpdateCredentialsAsync(KdbxDocument document, ITestableFile databaseFile, IEnumerable<ISecurityToken> tokens);
    }
}
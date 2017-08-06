// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.KeePass.Dom;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Contracts.KeePass
{
    public interface IKdbxWriter
    {
        KdbxHeaderData HeaderData { get; }

        /// <summary>
        /// Writes a document to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="document">The document to write.</param>
        /// <param name="token">A token allowing the operation to be cancelled.</param>
        /// <returns>Whether the write succeeded.</returns>
        Task<bool> WriteAsync(IOutputStream stream, KdbxDocument document, CancellationToken token);
    }
}

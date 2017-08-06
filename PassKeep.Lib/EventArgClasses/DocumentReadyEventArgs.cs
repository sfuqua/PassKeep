// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Files;
using System;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// An <see cref="EventArgs"/> extension that provides
    /// access to an <see cref="KdbxDocument"/> representing a KeePass
    /// document.
    /// </summary>
    public class DocumentReadyEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes the EventArgs with the provided parameters.
        /// </summary>
        /// <param name="document">A model representing the decrypted XML document.</param>
        /// <param name="candidate">The file corresponding to the opened document.</param>
        /// <param name="writer">An instance of IKdbxWriter that can write to the document.</param>
        /// <param name="rng">A random number generator that can encrypt protected strings for the document.</param>
        public DocumentReadyEventArgs(KdbxDocument document, IDatabaseCandidate candidate, IKdbxWriter writer, IRandomNumberGenerator rng)
        {
            Document = document;
            Candidate = candidate;
            Writer = writer;
            Rng = rng;
        }

        /// <summary>
        /// A decrypted XML document representing a KeePass document.
        /// </summary>
        public KdbxDocument Document { get; private set; }

        /// <summary>
        /// The file corresponding to the opened document.
        /// </summary>
        public IDatabaseCandidate Candidate { get; private set; }

        /// <summary>
        /// An object capable of writing to the new document.
        /// </summary>
        public IKdbxWriter Writer { get; private set; }

        /// <summary>
        /// A random number generator suitable for protecting strings in the new document.
        /// </summary>
        public IRandomNumberGenerator Rng { get; private set; }
    }
}

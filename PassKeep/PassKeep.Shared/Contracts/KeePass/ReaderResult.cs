using SariphLib.Mvvm;
using System.Diagnostics;

namespace PassKeep.Lib.Contracts.KeePass
{
    /// <summary>
    /// Serves as a wrapper for the KdbxParserCode enum -
    /// allows for sorting the errors into buckets and representing them
    /// as strings.
    /// </summary>
    public class ReaderResult : BindableBase
    {
        /// <summary>
        /// Representation of a successful read operation.
        /// </summary>
        public static readonly ReaderResult Success = new ReaderResult(KdbxParserCode.Success);

        // The internal parser code of this result.
        private KdbxParserCode _code;

        // Arguments used for a String representation of this result.
        private object[] args;

        /// <summary>
        /// Initializes the ReaderResult with the provided data.
        /// </summary>
        /// <param name="code">The parser code that this result represents.</param>
        /// <param name="args">Arguments that provide additional context for a result.</param>
        public ReaderResult(KdbxParserCode code, params object[] args)
        {
            this.Code = code;
            this.args = args;
        }

        /// <summary>
        /// The internal code wrapped by this result.
        /// </summary>
        public KdbxParserCode Code
        {
            get { return this._code; }
            private set
            {
                this._code = value;
                OnPropertyChanged("IsError");
            }
        }

        /// <summary>
        /// Whether this result represents a parse error (as opposed to a successful operation).
        /// </summary>
        public bool IsError
        {
            get { return this.Code != KdbxParserCode.Success; }
        }

        /// <summary>
        /// Represents a parse error due to an unknown header field.
        /// </summary>
        /// <param name="field">Identifier for the unknown header.</param>
        /// <returns>A ReaderResult representing a parse error due to an unknown header field.</returns>
        public static ReaderResult FromHeaderFieldUnknown(byte field)
        {
            return new ReaderResult(KdbxParserCode.HeaderFieldUnknown, field);
        }

        /// <summary>
        /// Represents a parse error due to a header data field being the wrong size.
        /// </summary>
        /// <param name="field">Identifier for the problematic header.</param>
        /// <param name="bytesReceived">The number of bytes actually in the header data.</param>
        /// <param name="requirement">An explanation of the size requirement.</param>
        /// <returns>A ReaderResult representing a parse error due to a bad header data size.</returns>
        public static ReaderResult FromHeaderDataSize(KdbxHeaderField field, int bytesReceived, string requirement)
        {
            return new ReaderResult(KdbxParserCode.HeaderDataSize, field, bytesReceived, requirement);
        }

        /// <summary>
        /// Represents a parse error due to header data that could not be interpreted.
        /// </summary>
        /// <param name="field">Identifier for the problematic header.</param>
        /// <param name="value"></param>
        /// <returns>A ReaderResult representing a parse error due to header data that could not be parsed.</returns>
        public static ReaderResult FromHeaderDataUnknown(KdbxHeaderField field, string value)
        {
            return new ReaderResult(KdbxParserCode.HeaderDataUnknown, field, value);
        }

        /// <summary>
        /// Gets a String representation of the parser result.
        /// </summary>
        /// <returns>A String representation of the parser result.</returns>
        public override string ToString()
        {
            switch (this.Code)
            {
                case KdbxParserCode.Success:
                    return "The operation completed successfully.";
                case KdbxParserCode.SignatureKP1:
                    return "The file is a KeePass 1.x database; only 2.x is supported.";
                case KdbxParserCode.SignatureKP2PR:
                    return "The file is a KeePass 2.x Prerelease database; only release version databases are supported.";
                case KdbxParserCode.SignatureInvalid:
                    return "The file has an invalid signature and is not a KeePass database.";
                case KdbxParserCode.Version:
                    return "The file was made using an unsupported KeePass 2.x version.";
                case KdbxParserCode.HeaderFieldDuplicate:
                    return string.Format("The database file contains a duplicated header field: {0}", args);
                case KdbxParserCode.HeaderFieldUnknown:
                    return string.Format("The database file contains an unknown header field: {0}", args);
                case KdbxParserCode.HeaderDataSize:
                    return string.Format("The database header field '{0}' has the wrong number of data bytes ({1}; {2})", args);
                case KdbxParserCode.HeaderDataUnknown:
                    return string.Format("The database header field '{0}' has an unknown or unsupported value of {1}.", args);
                case KdbxParserCode.HeaderMissing:
                    return string.Format("The database file did not initialize all required headers; it may be corrupt.");
                case KdbxParserCode.CouldNotDecrypt:
                    return string.Format("We were unable to decrypt the database file. Please check your credentials.");
                case KdbxParserCode.FirstBytesMismatch:
                    return string.Format("Decryption was unsuccessful; please double-check your security information.");
                case KdbxParserCode.CouldNotInflate:
                    return string.Format("We were unable to decompress the database file; it may be corrupt.");
                case KdbxParserCode.CouldNotParseXml:
                    return string.Format("We were unable to parse the database file XML; it may be corrupt.");
                case KdbxParserCode.OperationCancelled:
                    return "The operation was cancelled.";
                case KdbxParserCode.BadHeaderHash:
                    return "The database file has a corrupted header; it may have been tampered with.";
                case KdbxParserCode.UnableToReadFile:
                    return "Unable to open the file - if you're using SkyDrive, try again later or choose 'Make offline' in the SkyDrive app.";
                default:
                    Debug.Assert(false);
                    return "Unknown Error code";
            }
        }
    }
}

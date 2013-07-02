using System.Diagnostics;
using System.Runtime.Serialization;

namespace PassKeep.KeePassLib
{
    [DataContract]
    public class KeePassError
    {
        public static readonly KeePassError None = new KeePassError(KdbxParseError.None);

        [DataMember]
        public KdbxParseError Error
        {
            get;
            private set;
        }

        [DataMember]
        private object[] args;
        public KeePassError(KdbxParseError errorCode, params object[] args)
        {
            this.Error = errorCode;
            this.args = args;
        }

        public static KeePassError FromHeaderFieldUnknown(byte field)
        {
            return new KeePassError(KdbxParseError.HeaderFieldUnknown, field);
        }

        public static KeePassError FromHeaderDataSize(string field, int bytesReceived, string requirement)
        {
            return new KeePassError(KdbxParseError.HeaderDataSize, field, bytesReceived, requirement);
        }

        public static KeePassError FromHeaderDataUnknown(string field)
        {
            return new KeePassError(KdbxParseError.HeaderDataUnknown, field);
        }

        public override string ToString()
        {
            switch (Error)
            {
                case KdbxParseError.None:
                    return "The operation completed successfully.";
                case KdbxParseError.SignatureKP1:
                    return "The file is a KeePass 1.x database; only 2.x is supported.";
                case KdbxParseError.SignatureKP2PR:
                    return "The file is a KeePass 2.x Prerelease database; only release version databases are supported.";
                case KdbxParseError.SignatureInvalid:
                    return "The file has an invalid signature and is not a KeePass database.";
                case KdbxParseError.Version:
                    return "The file was made using an unsupported KeePass 2.x version.";
                case KdbxParseError.HeaderFieldDuplicate:
                    return string.Format("The database file contains a duplicated header field: {0}", args);
                case KdbxParseError.HeaderFieldUnknown:
                    return string.Format("The database file contains an unknown header field: {0}", args);
                case KdbxParseError.HeaderDataSize:
                    return string.Format("The database header field '{0}' has the wrong number of data bytes ({1}; {2})", args);
                case KdbxParseError.HeaderDataUnknown:
                    return string.Format("The database header field '{0}' has an unknown or unsupported value.", args);
                case KdbxParseError.HeaderMissing:
                    return string.Format("The database file did not initialize all required headers; it may be corrupt.");
                case KdbxParseError.CouldNotDecrypt:
                    return string.Format("We were unable to decrypt the database file. Please check your credentials.");
                case KdbxParseError.FirstBytesMismatch:
                    return string.Format("Decryption was unsuccessful; please double-check your security information.");
                case KdbxParseError.CouldNotInflate:
                    return string.Format("We were unable to decompress the database file; it may be corrupt.");
                case KdbxParseError.CouldNotParseXml:
                    return string.Format("We were unable to parse the database file XML; it may be corrupt.");
                case KdbxParseError.OperationCancelled:
                    return "The operation was cancelled.";
                case KdbxParseError.BadHeaderHash:
                    return "The database file has a corrupted header; it may have been tampered with.";
                case KdbxParseError.UnableToReadFile:
                    return "Unable to open the file - if you're using SkyDrive, try again later or choose 'Make offline' in the SkyDrive app.";
                default:
                    Debug.Assert(false);
                    return "Unknown Error code";
            }
        }
    }

    public enum KdbxParseError
    {
        None,
        SignatureKP1,
        SignatureKP2PR,
        SignatureInvalid,
        Version,
        HeaderFieldDuplicate,
        HeaderFieldUnknown,
        HeaderDataSize,
        HeaderDataUnknown,
        HeaderMissing,
        CouldNotDecrypt,
        FirstBytesMismatch,
        CouldNotInflate,
        CouldNotParseXml,
        MalformedXml,
        OperationCancelled,
        BadHeaderHash,
        UnableToReadFile,
    }
}

// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

namespace PassKeep.Lib.Contracts.KeePass
{
    public enum KdbxParserCode
    {
        Success,
        SignatureKP1,
        SignatureKP2PR,
        SignatureInvalid,
        Version,
        HeaderFieldUnknown,
        HeaderDataSize,
        HeaderDataUnknown,
        HeaderMissing,
        DataIntegrityProblem,
        CouldNotDecrypt,
        FirstBytesMismatch,
        CouldNotInflate,
        CouldNotDeserialize,
        MalformedXml,
        OperationCancelled,
        BadHeaderHash,
        UnableToReadFile,
        CouldNotRetrieveCredentials,
        CouldNotVerifyIdentity,
        BadVariantDictionary,
        TestFailure
    }
}

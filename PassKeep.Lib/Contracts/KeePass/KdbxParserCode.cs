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
        CouldNotDecrypt,
        FirstBytesMismatch,
        CouldNotInflate,
        CouldNotDeserialize,
        MalformedXml,
        OperationCancelled,
        BadHeaderHash,
        UnableToReadFile,
        CouldNotRetrieveCredentials,
        CouldNotVerifyIdentity
    }
}

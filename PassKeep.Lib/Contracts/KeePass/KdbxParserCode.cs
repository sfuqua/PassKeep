namespace PassKeep.Lib.Contracts.KeePass
{
    public enum KdbxParserCode
    {
        Success,
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
        CouldNotRetrieveCredentials,
        CouldNotVerifyIdentity
    }
}

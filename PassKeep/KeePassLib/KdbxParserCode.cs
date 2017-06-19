using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.KeePassLib
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.KeePassLib
{
    /// <summary>
    /// Serves as a wrapper for the KdbxParserCode enum -
    /// allows for sorting the errors into buckets and representing them
    /// as strings.
    /// </summary>
    public class ReaderResult
    {
        /// <summary>
        /// Representation of a successful read operation.
        /// </summary>
        public static readonly ReaderResult Success = new ReaderResult(KdbxParserCode.Success);

        // The internal parser code of this result.
        private readonly KdbxParserCode code;

        // Detailed information about the parse result.
        private readonly string details;

        /// <summary>
        /// A <see cref="ReaderResult"/> that simply wraps a <see cref="KdbxParserCode"/> with no further details.
        /// </summary>
        /// <param name="code"></param>
        public ReaderResult(KdbxParserCode code)
        {
            this.code = code;
            this.details = string.Empty;
        }

        /// <summary>
        /// A <see cref="ReaderResult"/> that simply wraps a <see cref="KdbxParserCode"/> with no further details.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="details"></param>
        public ReaderResult(KdbxParserCode code, string details)
        {
            this.code = code;
            this.details = details;
        }

        /// <summary>
        /// A <see cref="ReaderResult"/> that simply wraps a <see cref="KdbxParserCode"/> with no further details.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="ex"></param>
        public ReaderResult(KdbxParserCode code, Exception ex)
        {
            this.code = code;
            this.details = ex.Message;
        }

        /// <summary>
        /// The internal code wrapped by this result.
        /// </summary>
        public KdbxParserCode Code
        {
            get { return this.code; }
        }

        /// <summary>
        /// Whether this result represents a parse error (as opposed to a successful operation).
        /// </summary>
        public bool IsError
        {
            get { return Code != KdbxParserCode.Success; }
        }

        /// <summary>
        /// Helper to determine whether <see cref="Details"/> has a value, to avoid unnecessary
        /// conversions when binding.
        /// </summary>
        public bool HasDetails
        {
            get { return !string.IsNullOrEmpty(Details); }
        }

        /// <summary>
        /// Details on the error.
        /// </summary>
        public string Details
        {
            get { return this.details; }
        }

        /// <summary>
        /// Represents a parse error due to an unknown header field.
        /// </summary>
        /// <param name="field">Identifier for the unknown header.</param>
        /// <returns>A ReaderResult representing a parse error due to an unknown header field.</returns>
        public static ReaderResult FromHeaderFieldUnknown(byte field)
        {
            return new ReaderResult(KdbxParserCode.HeaderFieldUnknown, field.ToString());
        }

        /// <summary>
        /// Represents a parse error due to a header data field being the wrong size.
        /// </summary>
        /// <param name="field">Identifier for the problematic header.</param>
        /// <param name="bytesReceived">The number of bytes actually in the header data.</param>
        /// <param name="requirement">The size requirement in English.</param>
        /// <returns>A ReaderResult representing a parse error due to a bad header data size.</returns>
        public static ReaderResult FromHeaderDataSize(OuterHeaderField field, uint bytesReceived, string requirement)
        {
            return new ReaderResult(
                KdbxParserCode.HeaderDataSize,
                "field: " + field + ", sizeReq: " + requirement + ", got: " + bytesReceived
            );
        }

        /// <summary>
        /// Represents a parse error due to a header data field being the wrong size.
        /// </summary>
        /// <param name="field">Identifier for the problematic header.</param>
        /// <param name="bytesReceived">The number of bytes actually in the header data.</param>
        /// <param name="requirement">The size requirement in English.</param>
        /// <returns>A ReaderResult representing a parse error due to a bad header data size.</returns>
        public static ReaderResult FromHeaderDataSize(InnerHeaderField field, uint bytesReceived, string requirement)
        {
            return new ReaderResult(
                KdbxParserCode.HeaderDataSize,
                "field: " + field + ", sizeReq: " + requirement + ", got: " + bytesReceived
            );
        }

        /// <summary>
        /// Represents a parse error due to header data that could not be interpreted.
        /// </summary>
        /// <param name="field">Identifier for the problematic header.</param>
        /// <param name="value">The unsupported header value.</param>
        /// <returns>A ReaderResult representing a parse error due to header data that could not be parsed.</returns>
        public static ReaderResult FromHeaderDataUnknown(OuterHeaderField field, string value)
        {
            return new ReaderResult(
                KdbxParserCode.HeaderDataUnknown,
                "field: " + field + ", value: " + value
            );
        }

        /// <summary>
        /// Represents a parse error due to header data that could not be interpreted.
        /// </summary>
        /// <param name="field">Identifier for the problematic header.</param>
        /// <param name="value">The unsupported header value.</param>
        /// <returns>A ReaderResult representing a parse error due to header data that could not be parsed.</returns>
        public static ReaderResult FromHeaderDataUnknown(InnerHeaderField field, string value)
        {
            return new ReaderResult(
                KdbxParserCode.HeaderDataUnknown,
                "field: " + field + ", value: " + value
            );
        }

        /// <summary>
        /// Represents a parse error due to a header <see cref="VariantDictionary"/> that
        /// couldn't parse.
        /// </summary>
        /// <param name="exceptionText">Detailed information about the parse failure.</param>
        /// <returns>A ReaderResult representing a parse error due to a bad dictionary.</returns>
        public static ReaderResult FromBadVariantDictionary(string exceptionText)
        {
            return new ReaderResult(
                KdbxParserCode.BadVariantDictionary,
                "exception: " + exceptionText
            );
        }

        /// <summary>
        /// Represents a parse error due to unsupported XML.
        /// </summary>
        /// <param name="failure">Details on what went wrong.</param>
        /// <returns></returns>
        public static ReaderResult FromXmlParseFailure(string failure)
        {
            return new ReaderResult(KdbxParserCode.CouldNotDeserialize, failure);
        }

        /// <summary>
        /// A parse error to be used for testing.
        /// </summary>
        /// <param name="failure">Details.</param>
        /// <returns></returns>
        public static ReaderResult GenerateTestFailure(string failure)
        {
            return new ReaderResult(KdbxParserCode.TestFailure, failure);
        }
    }
}

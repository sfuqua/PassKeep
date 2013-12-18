using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.Models
{
    /// <summary>
    /// Represents a (key, value) pair where the value may or may not
    /// be "protected" internally.
    /// </summary>
    public interface IProtectedString : IKeePassSerializable
    {
        /// <summary>
        /// The (unique) Key used to identify this string.
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// The cleartext of the protected string.
        /// </summary>
        string ClearValue { get; set; }

        /// <summary>
        /// The raw text of the protected string. 
        /// If protected, it may be ciphertext. 
        /// Otherwise, it may be the cleartext.
        /// </summary>
        string RawValue { get; }

        /// <summary>
        /// Whether this string is currently protected.
        /// </summary>
        bool Protected { get; set; }

        /// <summary>
        /// Generates a unique (but identical) copy of this instance.
        /// </summary>
        /// <returns></returns>
        IProtectedString Clone();
    }
}

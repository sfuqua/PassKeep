using PassKeep.Lib.Contracts.Models;
using System;
using PassKeep.Lib.Contracts.KeePass;
using System.Xml.Linq;
using SariphLib.Mvvm;
using PassKeep.Lib.KeePass.IO;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// A barebones implementation of <see cref="IProtectedString"/> that notifies
    /// property changes only for ClearValue and the rest of the properties are no-ops.
    /// </summary>
    public sealed class UnprotectedString : BindableBase, IProtectedString
    {
        private string value;

        public UnprotectedString(string value)
        {
            ClearValue = value;
        }

        /// <summary>
        /// The value represented by this string.
        /// </summary>
        public string ClearValue
        {
            get
            {
                return this.value;
            }
            set
            {
                TrySetProperty(ref this.value, value);
            }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public string Key
        {
            get;
            set;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public bool Protected
        {
            get;
            set;
        }

        /// <summary>
        /// Same as <see cref="ClearValue"/>.
        /// </summary>
        public string RawValue
        {
            get
            {
                return ClearValue;
            }
        }

        /// <summary>
        /// Copies the <see cref="ClearValue"/>.
        /// </summary>
        /// <returns></returns>
        public IProtectedString Clone()
        {
            return new UnprotectedString(ClearValue);
        }

        /// <summary>
        /// Compares ClearValues.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(IProtectedString other)
        {
            return (ClearValue ?? string.Empty).CompareTo(other.ClearValue ?? string.Empty);
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="rng"></param>
        /// <returns></returns>
        public XElement ToXml(IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        {
            throw new NotImplementedException();
        }
    }
}

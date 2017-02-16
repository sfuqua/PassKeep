using System;

namespace PassKeep.Lib.KeePass.IO
{
    /// <summary>
    /// An attribute that specifies the minimum and maximum KDBX versions
    /// that support a given feature, header, etc.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public sealed class KdbxVersionSupportAttribute : Attribute
    {
        /// <summary>
        /// Initializes the attribute with no versioning.
        /// </summary>
        public KdbxVersionSupportAttribute()
        {
            Min = KdbxVersion.Unspecified;
            Max = KdbxVersion.Unspecified;
        }

        /// <summary>
        /// First KDBX version to support this capability.
        /// </summary>
        public KdbxVersion Min
        {
            get;
            set;
        }

        /// <summary>
        /// Last KDBX version to support this capability.
        /// </summary>
        public KdbxVersion Max
        {
            get;
            set;
        }

        /// <summary>
        /// Determines whether the specified version is within the range
        /// of this attribute.
        /// </summary>
        /// <param name="version">The version to test.</param>
        /// <returns>Whether <paramref name="version"/> is within 
        /// <see cref="Max"/> and <see cref="Min"/>.</returns>
        public bool Supports(KdbxVersion version)
        {
            if (Min != KdbxVersion.Unspecified && Min > version)
            {
                return false;
            }

            if (Max != KdbxVersion.Unspecified && Max < version)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// String form of the min and max versions.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[{Min}, {Max}]";
        }
    }
}

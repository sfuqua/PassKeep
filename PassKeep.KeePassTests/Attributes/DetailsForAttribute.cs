using System;

namespace PassKeep.KeePassTests.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class DetailsForAttribute : Attribute
    {
        /// <summary>
        /// Whether the ViewModel represents a new node (instead of an existing one).
        /// </summary>
        public bool IsNew
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether an existing node is being opened in read-only mode.
        /// </summary>
        public bool IsOpenedReadOnly
        {
            get;
            private set;
        }

        /// <summary>
        /// Simple initialization constructor.
        /// </summary>
        /// <param name="isNew">Whether the node is new or existing.</param>
        /// <param name="isOpenedReadOnly">Whether the node is opened in read-only mode.</param>
        public DetailsForAttribute(
            bool isNew = true,
            bool isOpenedReadOnly = false
        )
        {
            if (isNew && isOpenedReadOnly)
            {
                throw new ArgumentException("Cannot open a new node in read-only mode!");
            }

            this.IsNew = isNew;
            this.IsOpenedReadOnly = IsOpenedReadOnly;
        }
    }
}

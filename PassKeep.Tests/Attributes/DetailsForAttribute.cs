using System;

namespace PassKeep.Tests.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class DetailsForAttribute : Attribute
    {
        /// <summary>
        /// Whether the ViewModel represents a new child (instead of an existing one).
        /// </summary>
        public bool IsNew
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether an existing child is being opened in read-only mode.
        /// </summary>
        public bool IsOpenedReadOnly
        {
            get;
            private set;
        }

        /// <summary>
        /// Simple initialization constructor.
        /// </summary>
        /// <param name="isNew">Whether the child is new or existing.</param>
        /// <param name="isOpenedReadOnly">Whether the child is opened in read-only mode.</param>
        public DetailsForAttribute(
            bool isNew = true,
            bool isOpenedReadOnly = false
        )
        {
            if (isNew && isOpenedReadOnly)
            {
                throw new ArgumentException("Cannot open a new node in read-only mode!");
            }

            IsNew = isNew;
            IsOpenedReadOnly = IsOpenedReadOnly;
        }
    }
}

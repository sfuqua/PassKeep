using System;

namespace PassKeep.Tests.Attributes
{
    /// <summary>
    /// An Attribute that allows TestMethods to declare information about the database they rely on.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
    public class DatabaseInfoAttribute : Attribute
    {
        /// <summary>
        /// The password used to decrypt the database, or the empty string if one doesn't exist.
        /// </summary>
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the key file used to decrypt the database.
        /// </summary>
        public string KeyFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Whether this database represents a PassKeep sample database.
        /// </summary>
        public bool IsSample
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the database.
        /// </summary>
        public string DatabaseName
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether to use the default (valid) credentials for a database, or to override them.
        /// </summary>
        public bool UseDefaultCredentials
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the attribute.
        /// </summary>
        /// <param name="databaseName">The name of the database to use for this test.</param>
        /// <param name="useDefaultCredentials">Whether to use default (valid) credentials for the database.</param>
        public DatabaseInfoAttribute(string databaseName, bool useDefaultCredentials = true)
        {
            this.DatabaseName = databaseName;
            this.UseDefaultCredentials = useDefaultCredentials;
            this.Password = String.Empty;
            this.KeyFileName = null;
            this.IsSample = false;
        }
    }
}

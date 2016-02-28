using PassKeep.Lib.Contracts.Providers;
using System.Collections.Generic;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// A resource provider backed by an optional dictionary.
    /// </summary>
    public class MockResourceProvider : IResourceProvider
    {
        private readonly IDictionary<string, string> values;

        /// <summary>
        /// Initializes the resource provider with no strings.
        /// </summary>
        public MockResourceProvider()
        {
            this.values = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes the resource provider with the given dictionary.
        /// </summary>
        /// <param name="values">The strings to load the provider with.</param>
        public MockResourceProvider(IDictionary<string, string> values)
        {
            this.values = new Dictionary<string, string>(values);
        }

        /// <summary>
        /// Fetches the specified string if it exists, else "DUMMY".
        /// </summary>
        /// <param name="key">The string to fetch.</param>
        /// <returns>The specified string, else "DUMMY".</returns>
        public string GetString(string key)
        {
            if (this.values.ContainsKey(key))
            {
                return this.values[key];
            }

            return "DUMMY";
        }
    }
}

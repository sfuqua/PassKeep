// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PassKeep.Framework
{
    /// <summary>
    /// A type to be used for navigating between pages in the framework, that has an optional
    /// key to be used for resolving container types (when multiple constructors are available,
    /// for example) and parameters to be used for constructor param injection.
    /// </summary>
    public class NavigationParameter
    {
        /// <summary>
        /// Creates a NavigationParameter instance with the specified key and parameters.
        /// </summary>
        /// <param name="concreteTypeKey">An optional string to be used to decide between
        /// available IoC container types.</param>
        /// <param name="parameters">An optional list of key/value pairs to be used for
        /// parameter injection.</param>
        public NavigationParameter(object parameters = null, string concreteTypeKey = "")
        {
            DynamicParameters = NavigationParameter.GetResolverOverrides(parameters);
            ConcreteTypeKey = concreteTypeKey;
        }

        /// <summary>
        /// An enumeration of overriddden parameters for dependency injection purposes.
        /// </summary>
        public IEnumerable<ResolverOverride> DynamicParameters
        {
            get;
            private set;
        }

        /// <summary>
        /// A string serving as the key to a concrete type for IoC container resolution.
        /// </summary>
        public string ConcreteTypeKey
        {
            get;
            private set;
        }

        /// <summary>
        /// A helper function to turn an object (representing key-value pairs) into an enumeration
        /// of Unity ResolverOverrides.
        /// </summary>
        /// <param name="parameters">An object with named values for each override.</param>
        /// <returns>An enumeration of Unity ResolverOverrides extracted from the parameters.</returns>
        public static IEnumerable<ResolverOverride> GetResolverOverrides(object parameters)
        {
            if (parameters == null)
            {
                return new ResolverOverride[0];
            }
            else
            {
                // From the parameters object, get all properties.
                // Use the properties to construct a Unity ParameterOverride using each prop's
                // name and value.
                return parameters.GetType().GetRuntimeProperties()
                    .Select(
                        prop =>
                            new ParameterOverride(
                                prop.Name,
                                new InjectionParameter(prop.PropertyType, prop.GetValue(parameters))
                            )
                    );
            }
        }
    }
}

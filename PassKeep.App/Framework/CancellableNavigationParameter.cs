// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;

namespace PassKeep.Framework
{
    /// <summary>
    /// A navigation parameter to be used for navigates that can be canceled and resumed later.
    /// A callback is attached for a Page to reference when it finishes the navigate manually.
    /// </summary>
    public class CancellableNavigationParameter
    {
        /// <summary>
        /// Initializes the instance's properties.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="wrappedParameter"></param>
        public CancellableNavigationParameter(Action callback, object wrappedParameter)
        {
            Callback = callback;
            WrappedParameter = wrappedParameter;
        }

        /// <summary>
        /// The callback to invoke when the navigation resumes.
        /// </summary>
        public Action Callback
        {
            get;
            private set;
        }

        /// <summary>
        /// The navigation parameter to use for the resumed navigate.
        /// </summary>
        public object WrappedParameter
        {
            get;
            private set;
        }
    }
}

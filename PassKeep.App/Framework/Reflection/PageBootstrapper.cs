// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Diagnostics;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity;
using Unity.Resolution;

namespace PassKeep.Framework.Reflection
{
    /// <summary>
    /// Helper class for handling all the crazy reflection needed by the framework's conventions.
    /// </summary>
    public static class PageBootstrapper
    {
        /// <summary>
        /// Inspects the type of <paramref name="newContent"/> to determine the type of ViewModel to create,
        /// then creates it using <paramref name="container"/> and attaches it as the new page's DataContext.
        /// </summary>
        /// <param name="newContent">The page to wire up.</param>
        /// <param name="navParameter">The parameter used to navigate to the page.</param>
        /// <param name="container">The IoC container.</param>
        /// <param name="viewType">The computed view Type.</param>
        /// <param name="viewModelType">The computed ViewModel Type.</param>
        /// <returns></returns>
        public static IViewModel GenerateViewModel(PassKeepPage newContent, object navParameter, IUnityContainer container, out Type viewType, out Type viewModelType)
        {
            newContent.Trace("+");
            IViewModel contentViewModel = null;

            // Wire up the ViewModel
            // First, we figure out the ViewModel interface type
            viewType = newContent.GetType();
            Type viewBaseType = viewType.GetTypeInfo().BaseType;

            if (viewBaseType.Equals(typeof(PassKeepPage)))
            {
                // This is just a PassKeepPage, not a generic type. No ViewModel construction is necessary.
                DebugHelper.Assert(navParameter == null);
                viewModelType = null;
                return null;
            }

            Type genericPageType = viewBaseType.GetTypeInfo().BaseType;
            viewModelType = genericPageType.GenericTypeArguments[0];

            TypeInfo viewModelTypeInfo = viewModelType.GetTypeInfo();
            DebugHelper.Assert(typeof(IViewModel).GetTypeInfo().IsAssignableFrom(viewModelTypeInfo));

            
            if (navParameter != null)
            {
                if (viewModelTypeInfo.IsAssignableFrom(navParameter.GetType().GetTypeInfo()))
                {
                    contentViewModel = (IViewModel)navParameter;
                }
                else
                {
                    NavigationParameter parameter = navParameter as NavigationParameter;
                    DebugHelper.Assert(parameter != null);

                    ResolverOverride[] overrides = parameter.DynamicParameters.ToArray();

                    // We resolve the ViewModel (with overrides) from the container
                    if (String.IsNullOrEmpty(parameter.ConcreteTypeKey))
                    {
                        contentViewModel = (IViewModel)container.Resolve(viewModelType, overrides);
                    }
                    else
                    {
                        contentViewModel =
                            (IViewModel)container.Resolve(viewModelType, parameter.ConcreteTypeKey, overrides);
                    }
                }
            }
            else
            {
                contentViewModel = (IViewModel)container.Resolve(viewModelType);
            }

            contentViewModel.Logger = container.Resolve<IEventLogger>();
            newContent.DataContext = contentViewModel;

            newContent.Trace("-");
            return contentViewModel;
        }

        /// <summary>
        /// Inspects a page for event handlers for its ViewModel according to convention.
        /// </summary>
        /// <param name="newContent">The page with event handlers.</param>
        /// <param name="contentViewModel">The ViewModel with events.</param>
        /// <param name="viewType">The precomputed type of the view.</param>
        /// <param name="viewModelType">The precomputed type of the ViewModel.</param>
        /// <returns>A list of EventInfo/Delegate pairings representing the computed event handlers (so they can be detached later).</returns>
        public static IList<Tuple<EventInfo, Delegate>> WireViewModelEventHandlers(PassKeepPage newContent, IViewModel contentViewModel, Type viewType, Type viewModelType)
        {
            newContent.Trace("+");
            IList<Tuple<EventInfo, Delegate>> autoHandlers = new List<Tuple<EventInfo, Delegate>>();

            if (viewModelType != null)
            {
                IEnumerable<EventInfo> vmEvents = AggregateMembersForType<EventInfo>(viewModelType, t => t.GetRuntimeEvents());

                foreach (EventInfo evt in vmEvents)
                {
                    Type handlerType = evt.EventHandlerType;
                    DebugHelper.Trace($"Event name: {evt.Name}");
                    DebugHelper.Trace($"Handler type: {handlerType}");

                    // See if any potential listeners are opted in as event listeners using AutoWire
                    newContent.Trace("Generating candidates");
                    IList<MethodInfo> candidateHandlers =
                        (from candidate in AggregateMembersForType<MethodInfo>(viewType, t => t.GetRuntimeMethods())
                         let autoWireAttr = candidate.GetCustomAttribute<AutoWireAttribute>()
                         where autoWireAttr?.EventName == evt.Name
                         select candidate).ToList();
                    newContent.Trace($"Got {candidateHandlers.Count} candidates");

                    // Inherited methods can inflate this number, should probably figure out how to collapse them
                    //Dbg.Assert(candidateHandlers.Count < 2);
                    MethodInfo candidateHandler;
                    if (candidateHandlers.Count >= 1)
                    {
                        candidateHandler = candidateHandlers[0];

                        Type[] candidateHandlerParamTypes = candidateHandler.GetParameters().Select(p => p.ParameterType).ToArray();
                        MethodInfo match = handlerType.GetRuntimeMethod("Invoke", candidateHandlerParamTypes);
                        DebugHelper.Assert(match != null);
                    }
                    else
                    {
                        candidateHandler = null;
                    }

                    // If we got a matching method, hook it up!
                    if (candidateHandler != null)
                    {
                        Delegate handlerDelegate = candidateHandler.CreateDelegate(handlerType, newContent);
                        evt.AddEventHandler(contentViewModel, handlerDelegate);

                        // Save the delegate and the event for later, so we can unregister when we navigate away
                        autoHandlers.Add(new Tuple<EventInfo, Delegate>(evt, handlerDelegate));

                        DebugHelper.Trace($"Auto-wired EventHandler {handlerDelegate} for event {evt}");
                    }
                }
            }

            newContent.Trace("-");
            return autoHandlers;
        }

        /// <summary>
        /// Gets all members from the specified type, including inherited ones.
        /// </summary>
        /// <param name="derivedType">The type to probe for events.</param>
        /// <param name="reflector">The reflection function to use to collect members.</param>
        /// <returns>An enumerable of collected members.</returns>
        private static IEnumerable<T> AggregateMembersForType<T>(Type derivedType, Func<Type, IEnumerable<T>> reflector)
        {
            if (derivedType == typeof(object) || derivedType == null)
            {
                return new List<T>();
            }

            IList<T> members = reflector(derivedType).ToList();

            TypeInfo typInfo = derivedType.GetTypeInfo();
            if (typInfo.BaseType != null)
            {
                foreach (T member in AggregateMembersForType(derivedType.GetTypeInfo().BaseType, reflector))
                {
                    members.Add(member);
                }
            }
            else
            {
                foreach(Type @interface in typInfo.ImplementedInterfaces)
                {
                    // Intentionally not recursive for interfaces
                    foreach (T member in reflector(@interface))
                    {
                        members.Add(member);
                    }
                }
            }

            return members;
        }
    }
}

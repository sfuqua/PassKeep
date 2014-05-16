using PassKeep.Common;
using PassKeep.Lib.Contracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace PassKeep.Controls
{
    /// <summary>
    /// Represents a page of the app that is responsible for its own ViewModel,
    /// of a known type.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the ViewModel for this View</typeparam>
    public abstract class PassKeepPage<TViewModel> : PassKeepPage
        where TViewModel : class, IViewModel
    {
        // A list of delegates that were auto-attached (by convention) to ViewModel events, so that they
        // can be cleaned up later.
        private IList<Tuple<EventInfo, Delegate>> autoMethodHandlers = new List<Tuple<EventInfo, Delegate>>();

        /// <summary>
        /// Provides access to the ViewModel for this View
        /// </summary>
        public TViewModel ViewModel
        {
            get;
            protected set;
        }

        /// <summary>
        /// Loads the ViewModel from page state if it exists, otherwise tries to 
        /// cast one from the NavigationParameter.
        /// </summary>
        /// <remarks>Called by the View's NavigationHelper</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void navHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            bool gotVm = false;
            if (e.PageState != null)
            {
                if (e.PageState.ContainsKey("ViewModel"))
                {
                    this.ViewModel = e.PageState["ViewModel"] as TViewModel;
                    if (this.ViewModel == null)
                    {
                        throw new ViewModelException("Unable to cast ViewModel PageState to desired ViewModel type");
                    }

                    gotVm = true;
                }
            }

            if (!gotVm)
            {
                if (e.NavigationParameter == null)
                {
                    throw new ViewModelException("No NavigationParameter was specified");
                }

                this.ViewModel = e.NavigationParameter as TViewModel;
                if (this.ViewModel == null)
                {
                    throw new ViewModelException("Unable to cast NavigationParameter to desired ViewModel type");
                }
            }

            this.DataContext = this.ViewModel;

            // If possible, hook up Page event handlers to ViewModel events using convention-based approach.
            this.autoMethodHandlers.Clear();

            IEnumerable<EventInfo> vmEvents = this.ViewModel.GetType().GetRuntimeEvents();
            foreach(EventInfo evt in vmEvents)
            {
                Type handlerType = evt.EventHandlerType;
                MethodInfo invokeMethod = handlerType.GetRuntimeMethods().First(method => method.Name == "Invoke");

                string handlerName = handlerType.Name + "Handler";
                MethodInfo candidateHandler = this.GetType().GetRuntimeMethod(
                    handlerName,
                    invokeMethod.GetParameters().Select(parameter => parameter.ParameterType).ToArray()
                );

                if (candidateHandler != null)
                {
                    Delegate handlerDelegate = candidateHandler.CreateDelegate(handlerType, this);
                    evt.AddEventHandler(this.ViewModel, handlerDelegate);
                    this.autoMethodHandlers.Add(new Tuple<EventInfo, Delegate>(evt, handlerDelegate));

                    Debug.WriteLine("Attached auto-EventHandler {0} for event {1}", handlerDelegate, evt);
                }
            }
        }

        /// <summary>
        /// Persists the ViewModel to the page state and closes appbars.
        /// </summary>
        /// <remarks>Called by the View's NavigationHelper</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void navHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            e.PageState["ViewModel"] = this.ViewModel;
            BottomAppBar.IsSticky = false;
            BottomAppBar.IsOpen = false;

            while (this.autoMethodHandlers.Count > 0)
            {
                var autoHandler = this.autoMethodHandlers[0];
                this.autoMethodHandlers.RemoveAt(0);

                autoHandler.Item1.RemoveEventHandler(this.ViewModel, autoHandler.Item2);
                Debug.WriteLine("Removed auto-EventHandler {0} for event {1}", autoHandler.Item2, autoHandler.Item1.Name);
            }
        }
    }
}

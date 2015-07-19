using Microsoft.Practices.Unity;
using PassKeep.Framework.Reflection;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Infrastructure;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.System;
using Windows.UI.Core;

namespace PassKeep.Framework
{
    /// <summary>
    /// A PassKeepPage that is capable of hosting other pages - handles wiring up ViewModels for child pages as well as
    /// automatic event handlers, etc.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the ViewModel.</typeparam>
    public abstract class HostingPassKeepPage<TViewModel> : PassKeepPage<TViewModel>, IHostingPage
        where TViewModel : class, IViewModel
    {
        private TrackedPage trackedContent;

        public abstract Frame ContentFrame
        {
            get;
        }

        /// <summary>
        /// Allows a parent page to specify an IoC container.
        /// </summary>
        public IUnityContainer Container
        {
            private get;
            set;
        }

        /// <summary>
        /// A stored SystemNavigationManager for the instance.
        /// </summary>
        protected SystemNavigationManager SystemNavigationManager
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether this page or its content can navigate backwards.
        /// </summary>
        /// <returns></returns>
        public bool CanGoBack()
        {
            IHostingPage nestedPage = this.ContentFrame.Content as IHostingPage;
            if (nestedPage != null && nestedPage.CanGoBack())
            {
                return true;
            }
            
            if (this.ContentFrame.CanGoBack)
            {
                return true;
            }

            return Frame.CanGoBack;
        }

        /// <summary>
        /// Navigates the content frame if possible, otherwise the current frame.
        /// </summary>
        public void GoBack()
        {
            IHostingPage nestedPage = this.ContentFrame.Content as IHostingPage;
            if (nestedPage != null && nestedPage.CanGoBack())
            {
                nestedPage.GoBack();
            }
            else if (this.ContentFrame.CanGoBack)
            {
                this.ContentFrame.GoBack();
            }
            else if (this.Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        /// <summary>
        /// Handles passing accelerator keys down to child frames.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public override bool HandleAcceleratorKey(VirtualKey key, bool shift)
        {
            PassKeepPage child = this.ContentFrame?.Content as PassKeepPage;
            bool childHandled = (child != null ? child.HandleAcceleratorKey(key, shift) : false);

            if (childHandled)
            {
                return true;
            }
            
            return base.HandleAcceleratorKey(key, shift);
        }

        /// <summary>
        /// Handles bubbling up a child's StartedLoading event.
        /// </summary>
        /// <param name="sender">The child.</param>
        /// <param name="e">EventArgs for the child event.</param>
        protected virtual void ContentFrameStartedLoading(object sender, LoadingStartedEventArgs e)
        {
            RaiseStartedLoading(e);
        }

        /// <summary>
        /// Handles bubbling up a child's DoneLoading event.
        /// </summary>
        /// <param name="sender">The child.</param>
        /// <param name="e">EventArgs for the child event.</param>
        protected virtual void ContentFrameDoneLoading(object sender, EventArgs e)
        {
            RaiseDoneLoading();
        }

        /// <summary>
        /// Handle registering <see cref="ContentFrame"/>'s navigation event handlers.
        /// </summary>
        /// <param name="e">EventArgs for the navigation that loaded this page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.SystemNavigationManager = SystemNavigationManager.GetForCurrentView();

            Dbg.Assert(this.ContentFrame != null);
            this.ContentFrame.Navigating += TrackedFrame_Navigating;
            this.ContentFrame.Navigated += TrackedFrame_Navigated;

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Handles tearing down <see cref="ContentFrame"/> handlers.
        /// </summary>
        /// <param name="e">EventArgs for the navigation that is abandoning this page.</param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Dbg.Assert(this.ContentFrame != null);
            this.ContentFrame.Navigating -= TrackedFrame_Navigating;
            this.ContentFrame.Navigated -= TrackedFrame_Navigated;

            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// Invoked when the content Frame of the RootView is Navigating.
        /// </summary>
        /// <param name="sender">The content Frame.</param>
        /// <param name="e">NavigationEventArgs for the navigation.</param>
        private void TrackedFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            Frame thisFrame = sender as Frame;
            Dbg.Assert(thisFrame != null);

            // Tear down any content of the frame
            PassKeepPage content = thisFrame.Content as PassKeepPage;
            if (content != null)
            {
                UnloadFrameContent(content);
            }
        }

        /// <summary>
        /// Invoked when a tracked content Frame is done navigating.
        /// </summary>
        /// <remarks>
        /// Hooks up the new content Page's IOC logic.
        /// </remarks>
        /// <param name="sender">The content Frame.</param>
        /// <param name="e">NavigationEventArgs for the navigation.</param>
        private void TrackedFrame_Navigated(object sender, NavigationEventArgs e)
        {
            PassKeepPage newContent = e.Content as PassKeepPage;
            Dbg.Assert(newContent != null, "A content Frame should always navigate to a PassKeepPage");

            this.SystemNavigationManager.AppViewBackButtonVisibility =
                (CanGoBack() ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed);

            // Build up the new PassKeep Page
            HandleNewFrameContent(newContent, e.Parameter);
        }

        /// <summary>
        /// Creates a ViewModel for a new page and hooks up various event handlers.
        /// </summary>
        /// <param name="newContent">A page that was just navigated to.</param>
        /// <param name="navParameter">The parameter that was passed with the navigation.</param>
        private void HandleNewFrameContent(PassKeepPage newContent, object navParameter)
        {
            // TODO - Handle "back"
            Dbg.Assert(this.trackedContent == null);

            // If this is also a HostingPage, we need to pass down the IoC container
            IHostingPage newHostingContent = newContent as IHostingPage;
            if (newHostingContent != null)
            {
                newHostingContent.Container = this.Container;
            }

            // Pass down the the MessageBus
            newContent.MessageBus = this.MessageBus;

            // Hook up loading event handlers
            newContent.StartedLoading += ContentFrameStartedLoading;
            newContent.DoneLoading += ContentFrameDoneLoading;

            // Wire up the new view
            this.trackedContent = new TrackedPage(newContent, navParameter, this.Container);
        }

        /// <summary>
        /// Tears down event handlers associated with a page when it is going away.
        /// </summary>
        /// <param name="previousContent">The content that is navigating into oblivion.</param>
        private void UnloadFrameContent(PassKeepPage previousContent)
        {
            Dbg.Assert(previousContent != null);
            Dbg.Assert(this.trackedContent != null);

            // Tear down loading event handlers
            previousContent.StartedLoading -= ContentFrameStartedLoading;
            previousContent.DoneLoading -= ContentFrameDoneLoading;

            this.trackedContent.Dispose();
            this.trackedContent = null;
        }
    }
}

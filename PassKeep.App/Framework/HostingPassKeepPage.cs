// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using Microsoft.Practices.Unity;
using PassKeep.Framework.Reflection;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Diagnostics;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.System;
using Windows.UI.Core;
using System.Threading.Tasks;

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
            protected get;
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
            if (Frame.Content == this)
            {
                if (ContentFrame.Content is IHostingPage nestedPage && nestedPage.CanGoBack())
                {
                    return true;
                }

                if (ContentFrame.CanGoBack)
                {
                    return true;
                }
            }

            return Frame.CanGoBack;
        }

        /// <summary>
        /// Navigates the content frame if possible, otherwise the current frame.
        /// </summary>
        public void GoBack()
        {
            if (ContentFrame.Content is IHostingPage nestedPage && nestedPage.CanGoBack())
            {
                nestedPage.GoBack();
            }
            else if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
            else if (Frame.CanGoBack)
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
            PassKeepPage child = ContentFrame?.Content as PassKeepPage;
            bool childHandled = (child != null ? child.HandleAcceleratorKey(key, shift) : false);

            if (childHandled)
            {
                return true;
            }
            
            return base.HandleAcceleratorKey(key, shift);
        }

        /// <summary>
        /// Propagates app suspension handling to children.
        /// </summary>
        public override void HandleSuspend()
        {
            base.HandleSuspend();
            if (ContentFrame?.Content is PassKeepPage child)
            {
                child.HandleSuspend();
            }
        }

        /// <summary>
        /// Propagates app resume handling to children.
        /// </summary>
        public override void HandleResume()
        {
            base.HandleResume();
            if (ContentFrame?.Content is PassKeepPage child)
            {
                child.HandleResume();
            }
        }

        /// <summary>
        /// Handle registering <see cref="ContentFrame"/>'s navigation event handlers.
        /// </summary>
        /// <param name="e">EventArgs for the navigation that loaded this page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager = SystemNavigationManager.GetForCurrentView();

            DebugHelper.Assert(ContentFrame != null);
            ContentFrame.Navigated += TrackedFrame_Navigated;

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Handles tearing down <see cref="ContentFrame"/> handlers.
        /// </summary>
        /// <param name="e">EventArgs for the navigation that is abandoning this page.</param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (!e.Cancel)
            {
                DebugHelper.Assert(ContentFrame != null);
                ContentFrame.Navigated -= TrackedFrame_Navigated;
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
        private async void TrackedFrame_Navigated(object sender, NavigationEventArgs e)
        {
            PassKeepPage newContent = e.Content as PassKeepPage;
            DebugHelper.Assert(newContent != null, "A content Frame should always navigate to a PassKeepPage");

            SystemNavigationManager.AppViewBackButtonVisibility =
                (CanGoBack() ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed);

            // Build up the new PassKeep Page
            await HandleNewFrameContent(newContent, e.Parameter);
        }

        /// <summary>
        /// Creates a ViewModel for a new page and hooks up various event handlers.
        /// </summary>
        /// <param name="newContent">A page that was just navigated to.</param>
        /// <param name="navParameter">The parameter that was passed with the navigation.</param>
        private async Task HandleNewFrameContent(PassKeepPage newContent, object navParameter)
        {
            if (this.trackedContent  != null)
            {
                await UnloadFrameContent(this.trackedContent);
                this.trackedContent = null;
            }

            // If this is also a HostingPage, we need to pass down the IoC container
            if (newContent is IHostingPage newHostingContent)
            {
                newHostingContent.Container = Container;
            }

            // Pass down the the MessageBus
            newContent.MessageBus = MessageBus;

            // Wire up the new view
            this.trackedContent = new TrackedPage(newContent, navParameter, Container);
            await this.trackedContent.InitialActivation;
        }

        /// <summary>
        /// Tears down event handlers associated with a page when it is going away.
        /// </summary>
        /// <param name="previousContent">The content that is navigating into oblivion.</param>
        private async Task UnloadFrameContent(TrackedPage previousContent)
        {
            DebugHelper.Assert(previousContent != null);
            await this.trackedContent.CleanupAsync();
        }
    }
}

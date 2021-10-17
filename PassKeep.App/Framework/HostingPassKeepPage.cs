// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Framework.Reflection;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Diagnostics;
using System;
using System.Threading.Tasks;
using Unity;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
        /// <summary>
        /// Indicates that frame content has changed.
        /// </summary>
        protected event EventHandler CanGoBackChanged;

        private bool hasOwnCommandBar = false;
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
            bool childHandled = (ContentFrame?.Content is PassKeepPage child ? child.HandleAcceleratorKey(key, shift) : false);

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
        /// Invokes <see cref="CanGoBackChanged"/>.
        /// </summary>
        protected void RaiseCanGoBackChanged()
        {
            CanGoBackChanged?.Invoke(this, EventArgs.Empty);
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
            this.hasOwnCommandBar = HasActiveCommandBar;
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
            Trace("+");
            PassKeepPage newContent = e.Content as PassKeepPage;
            DebugHelper.Assert(newContent != null, "A content Frame should always navigate to a PassKeepPage");

            // Build up the new PassKeep Page
            await HandleNewFrameContent(newContent, e.Parameter);
            RaiseCanGoBackChanged();

            if (!this.hasOwnCommandBar)
            {
                if (newContent.HasActiveCommandBar)
                {
                    CommandBar = newContent.CommandBar;
                }
                else
                {
                    CommandBar = null;
                }
            }
            Trace("-");
        }

        private void HandleNewChildCommandBar(object sender, EventArgs e)
        {
            Trace("+");
            if (!this.hasOwnCommandBar)
            {
                if (((TrackedPage)sender).Page?.HasActiveCommandBar == true)
                {
                    Trace("Updating CommandBar");
                    CommandBar = ((TrackedPage)sender).Page.CommandBar;
                    Trace("CommandBar updated");
                }
                else
                {
                    Trace("Clearing CommandBar");
                    CommandBar = null;
                    Trace("CommandBar cleared");
                }
            }
            Trace("-");
        }

        /// <summary>
        /// Creates a ViewModel for a new page and hooks up various event handlers.
        /// </summary>
        /// <param name="newContent">A page that was just navigated to.</param>
        /// <param name="navParameter">The parameter that was passed with the navigation.</param>
        private async Task HandleNewFrameContent(PassKeepPage newContent, object navParameter)
        {
            Trace("+");
            if (this.trackedContent != null)
            {
                await UnloadFrameContent(this.trackedContent);
                this.trackedContent.CommandBarChanged -= HandleNewChildCommandBar;
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
            Trace($"Constructing TrackedPage for {newContent.GetType().Name}");
            this.trackedContent = new TrackedPage(newContent, navParameter, Container);
            Trace("Construction finished");
            await this.trackedContent.InitialActivation;
            Trace("Activation finished");
            this.trackedContent.CommandBarChanged += HandleNewChildCommandBar;
            Trace("-");
        }

        /// <summary>
        /// Tears down event handlers associated with a page when it is going away.
        /// </summary>
        /// <param name="previousContent">The content that is navigating into oblivion.</param>
        private async Task UnloadFrameContent(TrackedPage previousContent)
        {
            Trace($"Unloading content for {previousContent.Page.GetType().Name}");
            DebugHelper.Assert(previousContent != null);
            await this.trackedContent.CleanupAsync();
        }
    }
}

using PassKeep.Framework.Messages;
using SariphLib.Infrastructure;
using SariphLib.Messaging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.Framework
{
    /// <summary>
    /// The base Page type used by PassKeep.
    /// </summary>
    public abstract class BasePassKeepPage : Page, IListener
    {
        private static readonly Action NoOp = () => { };
        private ResourceLoader resourceLoader;
        private IDictionary<string, MethodInfo> messageSubscriptions;

        protected BasePassKeepPage()
        {
            this.resourceLoader = ResourceLoader.GetForCurrentView();
            this.messageSubscriptions = new Dictionary<string, MethodInfo>();
        }

        /// <summary>
        /// Provides public set access to the message bus and read access for subclasses.
        /// </summary>
        public MessageBus MessageBus
        {
            protected get;
            set;
        }

        /// <summary>
        /// Asynchronously deals with the specified message.
        /// </summary>
        /// <typeparam name="T">The type of message.</typeparam>
        /// <param name="message">The type of message.</param>
        /// <returns>A task representing the work to be done on the message.</returns>
        public Task HandleMessage(IMessage message)
        {
            if (!this.messageSubscriptions.ContainsKey(message.Name))
            {
                throw new ArgumentException("No such message handler exists", nameof(message));
            }

            return (Task)this.messageSubscriptions[message.Name].Invoke(this, new object[] { message });
        }

        /// <summary>
        /// Gets a key from the ResourceLoader.
        /// </summary>
        /// <param name="resourceKey">The key of the string to fetch.</param>
        /// <returns>A localized string.</returns>
        public string GetString(string resourceKey)
        {
            return this.resourceLoader.GetString(resourceKey);
        }

        /// <summary>
        /// Displays a file picker in the Documents library with any extension.
        /// </summary>
        /// <param name="gotFileCallback">Callback to invoke with the picked file.</param>
        /// <param name="cancelledCallback">Callback to invoke if the user pressed 'cancel'.</param>
        protected async Task PickFile(Action<StorageFile> gotFileCallback, Action cancelledCallback)
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            // Not all databases end in .kdbx
            picker.FileTypeFilter.Add("*");

            StorageFile pickedFile = await picker.PickSingleFileAsync();
            if (pickedFile == null)
            {
                Dbg.Trace("User cancelled the file picker.");
                cancelledCallback();
            }
            else
            {
                Dbg.Trace("User selected a file via the picker.");
                gotFileCallback(pickedFile);
            }
        }

        /// <summary>
        /// Displays a file picker in the Documents library with any extension.
        /// </summary>
        /// <param name="gotFileCallback">Callback to invoke with the picked file.</param>
        protected async Task PickFile(Action<StorageFile> gotFileCallback)
        {
            await PickFile(gotFileCallback, PassKeepPage.NoOp);
        }


        /// <summary>
        /// Registers this page to listen for the provided message types.
        /// </summary>
        /// <param name="messageTypes">The types to listen for.</param>
        protected void BootstrapMessageSubscriptions(params Type[] messageTypes)
        {
            foreach (Type t in messageTypes)
            {
                string name = MessageBase.GetName(t);

                MethodInfo method = this.GetType().GetTypeInfo().GetDeclaredMethod($"Handle{name}");
                Dbg.Assert(method != null, $"Handler for message {name} should be declared");

                ParameterInfo[] parameters = method.GetParameters();
                Dbg.Assert(parameters.Length == 1, "Message handlers must take one parameter");
                Dbg.Assert(parameters[0].ParameterType == t, "Message handler parameter type must match expected message type");
                Dbg.Assert(method.ReturnType.Equals(typeof(Task)), "Message handles must return a task");

                Dbg.Assert(!this.messageSubscriptions.ContainsKey(name));
                this.messageSubscriptions[name] = method;

                this.MessageBus.Subscribe(name, this);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            InputPane.GetForCurrentView().Showing += InputPaneShowingHandler;
            InputPane.GetForCurrentView().Hiding += InputPaneHidingHandler;

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Unsubscribes from messages when this page is going away.
        /// </summary>
        /// <param name="e">EventArgs for the navigation.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            InputPane.GetForCurrentView().Showing -= InputPaneShowingHandler;
            InputPane.GetForCurrentView().Hiding -= InputPaneHidingHandler;

            foreach (string messageName in this.messageSubscriptions.Keys)
            {
                this.MessageBus.Unsubscribe(messageName, this);
                this.messageSubscriptions.Remove(messageName);
            }

            base.OnNavigatedFrom(e);
        }

        // Helper to hide the app bar when the soft keyboard is showing -
        // works around a bug where it will occlude textboxes.
        private void InputPaneShowingHandler(InputPane pane, InputPaneVisibilityEventArgs args)
        {
            if (this.BottomAppBar != null)
            {
                this.BottomAppBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void InputPaneHidingHandler(InputPane pane, InputPaneVisibilityEventArgs args)
        {
            if (this.BottomAppBar != null)
            {
                this.BottomAppBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
    }
}

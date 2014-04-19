using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Input;
using Windows.System;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Represents a user's "position" within their document, including group breadcrumbs
    /// leading to the current group.
    /// </summary>
    public sealed class  DatabaseNavigationViewModel : BindableBase, IDatabaseNavigationViewModel
    {
        private ObservableCollection<IKeePassGroup> breadcrumbs;
        private Uri activeUri;

        private ReadOnlyObservableCollection<IKeePassGroup> _breadcrumbs;
        private ActionCommand _urlLaunchCommand;
        private IKeePassEntry _activeLeaf;

        /// <summary>
        /// Initializes the ViewModel.
        /// </summary>
        public DatabaseNavigationViewModel()
        {
            this.breadcrumbs = new ObservableCollection<IKeePassGroup>();
            this._breadcrumbs = new ReadOnlyObservableCollection<IKeePassGroup>(this.breadcrumbs);
            this.activeUri = null;

            this._urlLaunchCommand = new ActionCommand(CanLaunchUri, DoLaunchUri);
        }

        /// <summary>
        /// Raised when the visible leaf nodes of the current position change.
        /// </summary>
        public event EventHandler LeavesChanged;
        private void RaiseLeavesChanged()
        {
            if (LeavesChanged != null)
            {
                LeavesChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// The collection of groups leading up to (and including) the current position in the tree.
        /// </summary>
        public ReadOnlyObservableCollection<IKeePassGroup> Breadcrumbs
        {
            get { return this._breadcrumbs; }
        }

        /// <summary>
        /// The last group in the breadcrumb trail - the group the user is currently exploring.
        /// </summary>
        public IKeePassGroup ActiveGroup
        {
            get
            {
                if (this.Breadcrumbs == null || this.Breadcrumbs.Count == 0)
                {
                    return null;
                }
                return this.Breadcrumbs[this.Breadcrumbs.Count - 1];
            }
        }

        /// <summary>
        /// The entry the user is investigating, if one exists.
        /// </summary>
        public IKeePassEntry ActiveLeaf
        {
            get { return this._activeLeaf; }
            private set
            {
                if(SetProperty(ref this._activeLeaf, value))
                {
                    // If the ActiveLeaf has changed, update the activeUri
                    if (value == null)
                    {
                        this.activeUri = null;
                    }
                    else
                    {
                        try
                        {
                            // Use the OverrideUrl if one is available, otherwise the URL
                            if (!String.IsNullOrWhiteSpace(value.OverrideUrl))
                            {
                                this.activeUri = new Uri(value.OverrideUrl, UriKind.Absolute);
                            }
                            else
                            {
                                this.activeUri = new Uri(value.Url.ClearValue, UriKind.Absolute);
                            }
                        }
                        catch (Exception)
                        {
                            this.activeUri = null;
                        }
                    }

                    this._urlLaunchCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// An ICommand responsible for launching an entry's URL.
        /// </summary>
        public ICommand UrlLaunchCommand
        {
            get { return this._urlLaunchCommand; }
        }

        /// <summary>
        /// Clears out the ActiveLeaf.
        /// </summary>
        public void Prune()
        {
            this.ActiveLeaf = null;
        }

        /// <summary>
        /// Sets the last Breadcrumb to an Entry's parent, resets the leaves, and flags the Entry as active.
        /// </summary>
        /// <param name="entry">The entry to activate.</param>
        public void SetEntry(IKeePassEntry entry)
        {
            if (entry == null)
            {
                SetGroup(null);
            }
            else
            {
                SetGroup(entry.Parent);
            }

            this.ActiveLeaf = entry;
        }

        /// <summary>
        /// Updates the Breadcrumb list and resets all the leaves (including the active one).
        /// </summary>
        /// <param name="group">The group to activate.</param>
        public void SetGroup(IKeePassGroup group)
        {
            // If this is an effective no-op, do nothing.
            if (group == this.ActiveGroup)
            {
                return;
            }

            int originalChildCount = 0;

            // Remove the children changed handler from the current group.
            if (this.ActiveGroup != null)
            {
                ((INotifyCollectionChanged)this.ActiveGroup.Children).CollectionChanged -= ChildrenChangedHandler;
                originalChildCount = this.ActiveGroup.Children.Count;
            }

            // Are we clearing everything?
            if (group == null)
            {
                this.breadcrumbs.Clear();
            }
            else
            {
                if (group.Parent == null || this.breadcrumbs.Count == 0 || !group.Parent.Equals(ActiveGroup))
                {
                    // Either: 
                    // * The Group has no parent
                    // * There are no breadcrumbs
                    // * The Group is not a child of the last Group
                    this.breadcrumbs.Clear();

                    while (group != null)
                    {
                        this.breadcrumbs.Insert(0, group);
                        group = group.Parent;
                    }
                }
                else
                {
                    // The Group is a direct child of the last Group
                    this.breadcrumbs.Add(group);
                }

                ((INotifyCollectionChanged)this.ActiveGroup.Children).CollectionChanged += ChildrenChangedHandler;
            }

            if (this.ActiveLeaf != null && group != this.ActiveLeaf.Parent)
            {
                // Remove the ActiveLeaf if we're changing the ActiveGroup
                this.ActiveLeaf = null;
            }

            OnPropertyChanged("ActiveGroup");

            if (originalChildCount != 0 || (this.ActiveGroup != null && this.ActiveGroup.Children.Count != 0))
            {
                RaiseLeavesChanged();
            }
        }

        /// <summary>
        /// Handles raising the LeavesChanged event when the active group has a change to its children.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChildrenChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaiseLeavesChanged();
        }

        #region ActionCommand callbacks

        /// <summary>
        /// CanExecute callback for the UriLaunchCommand - determines whether the current entry URI is launchable.
        /// </summary>
        /// <returns>Whether there is a current, valid entry URI that can be launched.</returns>
        private bool CanLaunchUri()
        {
            return this.ActiveLeaf != null && this.activeUri != null;
        }

        /// <summary>
        /// Execution action for the UriLaunchCommand - attempts to launch the current entry URI.
        /// </summary>
        private async void DoLaunchUri()
        {
            Debug.Assert(this.CanLaunchUri());
            if (!this.CanLaunchUri())
            {
                throw new InvalidOperationException("The ViewModel is not in a state that can launch an entry URI!");
            }

            await Launcher.LaunchUriAsync(this.activeUri);
        }

        #endregion
    }
}

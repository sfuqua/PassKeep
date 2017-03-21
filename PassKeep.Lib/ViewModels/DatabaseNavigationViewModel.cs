using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Models;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Windows.System;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Represents a user's "position" within their document, including group breadcrumbs
    /// leading to the current group.
    /// </summary>
    public sealed class  DatabaseNavigationViewModel : AbstractViewModel, IDatabaseNavigationViewModel
    {
        private ObservableCollection<Breadcrumb> breadcrumbs;
        private Uri activeUri;

        private ReadOnlyObservableCollection<Breadcrumb> _breadcrumbs;
        private ActionCommand _urlLaunchCommand;
        private IKeePassEntry _activeLeaf;

        /// <summary>
        /// Initializes the ViewModel.
        /// </summary>
        public DatabaseNavigationViewModel()
        {
            this.breadcrumbs = new ObservableCollection<Breadcrumb>();
            this._breadcrumbs = new ReadOnlyObservableCollection<Breadcrumb>(this.breadcrumbs);
            this.activeUri = null;

            this._urlLaunchCommand = new ActionCommand(CanLaunchUri, DoLaunchUri);
        }

        /// <summary>
        /// Raised when the visible leaf nodes of the current position change.
        /// </summary>
        public event EventHandler LeavesChanged;
        private void RaiseLeavesChanged()
        {
            LeavesChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// The collection of groups leading up to (and including) the current position in the tree.
        /// </summary>
        public ReadOnlyObservableCollection<Breadcrumb> Breadcrumbs
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
                if (Breadcrumbs == null || Breadcrumbs.Count == 0)
                {
                    return null;
                }
                return Breadcrumbs[Breadcrumbs.Count - 1].Group;
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
                if(TrySetProperty(ref this._activeLeaf, value))
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
            ActiveLeaf = null;
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

            ActiveLeaf = entry;
        }

        /// <summary>
        /// Updates the Breadcrumb list and resets all the leaves (including the active one).
        /// </summary>
        /// <param name="group">The group to activate.</param>
        public void SetGroup(IKeePassGroup group)
        {
            // If this is an effective no-op, do nothing.
            if (group == ActiveGroup)
            {
                return;
            }

            int originalChildCount = 0;

            // Remove the children changed handler from the current group.
            if (ActiveGroup != null)
            {
                ((INotifyCollectionChanged)ActiveGroup.Children).CollectionChanged -= ChildrenChangedHandler;
                originalChildCount = ActiveGroup.Children.Count;
            }

            // Are we clearing everything?
            if (group == null)
            {
                this.breadcrumbs.Clear();
            }
            else
            {
                // Are we navigating upwards from the current tree?
                if (ActiveGroup != null && ActiveGroup.HasAncestor(group))
                {
                    int breadcrumbLastIndex = this.breadcrumbs.Count - 1;
                    // Pop off breadcrumbs until we're done
                    while (this.breadcrumbs[breadcrumbLastIndex].Group != group)
                    {
                        this.breadcrumbs.RemoveAt(breadcrumbLastIndex--);
                    }
                }
                // Are we navigating down a level to a new child of the current Group?
                else if (group.Parent != null && group.Parent.Equals(ActiveGroup))
                {
                    this.breadcrumbs.Add(new Breadcrumb(group));
                }
                // Something more catastrophic is happened, so just reset the list
                else
                {
                    this.breadcrumbs.Clear();

                    while (group != null)
                    {
                        this.breadcrumbs.Insert(0, new Breadcrumb(group, (group.Parent == null)));
                        group = group.Parent;
                    }
                }

                ((INotifyCollectionChanged)ActiveGroup.Children).CollectionChanged += ChildrenChangedHandler;
            }

            if (ActiveLeaf != null && group != ActiveLeaf.Parent)
            {
                // Remove the ActiveLeaf if we're changing the ActiveGroup
                ActiveLeaf = null;
            }

            OnPropertyChanged(nameof(ActiveGroup));

            if (originalChildCount != 0 || (ActiveGroup != null && ActiveGroup.Children.Count != 0))
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
            return ActiveLeaf != null && this.activeUri != null;
        }

        /// <summary>
        /// Execution action for the UriLaunchCommand - attempts to launch the current entry URI.
        /// </summary>
        private async void DoLaunchUri()
        {
            Dbg.Assert(CanLaunchUri());
            if (!CanLaunchUri())
            {
                throw new InvalidOperationException("The ViewModel is not in a state that can launch an entry URI!");
            }

            await Launcher.LaunchUriAsync(this.activeUri);
        }

        #endregion
    }
}

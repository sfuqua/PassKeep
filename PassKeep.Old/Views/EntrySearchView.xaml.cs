﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PassKeep.Common;
using System.Diagnostics;
using System.Collections.ObjectModel;
using PassKeep.ViewModels;
using Windows.ApplicationModel.Search;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;

// The Search Contract item template is documented at http://go.microsoft.com/fwlink/?LinkId=234240

namespace PassKeep.Views
{
    /// <summary>
    /// This page displays search results when a global search is directed to this application.
    /// </summary>
    public sealed partial class EntrySearchView : EntrySearchViewBase
    {
        private ICollection<IKeePassNode> _all;
        private ICollection<IKeePassGroup> _groups;
        private ICollection<IKeePassEntry> _entries;

        public override bool IsProtected
        {
            get { return false; }
        }

        public override bool IsUnsafe
        {
            get { return false; }
        }

        public override bool SearchOnKeypress
        {
            get
            {
                return true;
            }
        }

        public EntrySearchView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void navHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            base.navHelper_LoadState(sender, e);

            // TODO: Application-specific searching logic.  The search process is responsible for
            //       creating a list of user-selectable result categories:
            //
            //       filterList.Add(new SearchFilter("<filter name>", <result count>));
            //
            //       Only the first filter, typically "All", should pass true as a third argument in
            //       order to start in an active state.  Results for the active filter are provided
            //       in Filter_SelectionChanged below.

            var filterList = new List<SearchFilter>();
            if (ViewModel.DatabaseViewModel != null)
            {
                var unfiltered = ViewModel.DatabaseViewModel.GetAll().ToList();

                _all = unfiltered.Where(
                    s => s.MatchesQuery(ViewModel.Query)
                ).ToList();
                filterList.Add(new SearchFilter("All", _all.Count(), true));

                _entries = _all.Where(s => s is IKeePassEntry).Cast<IKeePassEntry>().ToList();
                filterList.Add(new SearchFilter("Only Entries", _entries.Count()));

                _groups = _all.Where(s => s is IKeePassGroup && !(s is IKeePassEntry)).Cast<IKeePassGroup>().ToList();
                filterList.Add(new SearchFilter("Only Groups", _groups.Count()));
            }
            else
            {
                filterList.Add(new SearchFilter("All", 0, true));
            }

            this.ViewModel.Filters = filterList;
            this.ViewModel.ShowFilters = filterList.Count > 1;
        }

        /// <summary>
        /// Invoked when a filter is selected using the ComboBox in snapped view state.
        /// </summary>
        /// <param name="sender">The ComboBox instance.</param>
        /// <param name="e">Event data describing how the selected filter was changed.</param>
        void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Determine what filter was selected
            var selectedFilter = e.AddedItems.FirstOrDefault() as SearchFilter;
            if (selectedFilter != null)
            {
                // Mirror the results into the corresponding SearchFilter object to allow the
                // RadioButton representation used when not snapped to reflect the change
                selectedFilter.Active = true;

                switch(selectedFilter.Name)
                {
                    case "All":
                        this.DefaultViewModel["Results"] = _all;
                        break;
                    case "Only Entries":
                        this.DefaultViewModel["Results"] = _entries;
                        break;
                    case "Only Groups":
                        this.DefaultViewModel["Results"] = _groups;
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }

                // Ensure results are found
                object results;
                ICollection resultsCollection;
                if (this.DefaultViewModel.TryGetValue("Results", out results) &&
                    (resultsCollection = results as ICollection) != null &&
                    _all.Count != 0)
                {
                    VisualStateManager.GoToState(this, "ResultsFound", true);
                    return;
                }
            }

            // Display informational text when there are no search results.
            VisualStateManager.GoToState(this, "NoResultsFound", true);
        }

        /// <summary>
        /// Invoked when a filter is selected using a RadioButton when not snapped.
        /// </summary>
        /// <param name="sender">The selected RadioButton instance.</param>
        /// <param name="e">Event data describing how the RadioButton was selected.</param>
        void Filter_Checked(object sender, RoutedEventArgs e)
        {
            // Mirror the change into the CollectionViewSource used by the corresponding ComboBox
            // to ensure that the change is reflected when snapped
            if (filtersViewSource.View != null)
            {
                var filter = (sender as FrameworkElement).DataContext;
                filtersViewSource.View.MoveCurrentTo(filter);
            }
        }

        /// <summary>
        /// View model describing one of the filters available for viewing search results.
        /// </summary>
        

        private void ItemClick(object sender, ItemClickEventArgs e)
        {
            IKeePassNode item = e.ClickedItem as IKeePassNode;
            Debug.Assert(item != null);

            if (ViewModel.DatabaseViewModel != null)
            {
                ViewModel.DatabaseViewModel.Select(item, true);
                Navigator.Navigate(typeof(DatabaseView), ViewModel.DatabaseViewModel);
            }
        }
    }
}
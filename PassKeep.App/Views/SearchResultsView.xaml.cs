using PassKeep.Common;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.ViewBases;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PassKeep.Views
{
    /// <summary>
    /// This page displays search results when a global search is directed to this application.
    /// </summary>
    public sealed partial class SearchResultsView : SearchResultsViewBase
    {
        private ICollection<IKeePassNode> allNodes;
        private ICollection<IKeePassNode> allGroups;
        private ICollection<IKeePassNode> allEntries;

        public SearchResultsView()
        {
            this.InitializeComponent();
        }

        public override IList<ICommandBarElement> GetSecondaryCommandBarElements()
        {
            return null;
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
        protected override void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var queryText = e.NavigationParameter as String;

            // TODO: Application-specific searching logic.  The search process is responsible for
            //       creating a list of user-selectable result categories:
            //
            //       filterList.Add(new Filter("<filter name>", <result count>));
            //
            //       Only the first filter, typically "All", should pass true as a third argument in
            //       order to start in an active state.  Results for the active filter are provided
            //       in Filter_SelectionChanged below.

            this.allNodes = this.ViewModel.GetAllNodes();
            this.allEntries = this.allNodes.Where(node => node is IKeePassEntry).ToList();
            this.allGroups = this.allNodes.Where(node => node is IKeePassGroup).ToList();

            List<SearchFilter> filterList = new List<SearchFilter>
            {
                new SearchFilter("All", this.allNodes.Count, true),
                new SearchFilter("Only Entries", this.allEntries.Count, false),
                new SearchFilter("Only Groups", this.allGroups.Count, false),
            };

            // Communicate results through the view model
            this.ViewModel.Filters = filterList;
            this.ViewModel.ShowFilters = filterList.Count > 1;
        }

        /// <summary>
        /// Invoked when a filter is selected using a RadioButton when not snapped.
        /// </summary>
        /// <param name="sender">The selected RadioButton instance.</param>
        /// <param name="e">Event data describing how the RadioButton was selected.</param>
        void Filter_Checked(object sender, RoutedEventArgs e)
        {
            var filter = (sender as FrameworkElement).DataContext;

            // Mirror the change into the CollectionViewSource.
            // This is most likely not needed.
            if (filtersViewSource.View != null)
            {
                filtersViewSource.View.MoveCurrentTo(filter);
            }

            // Determine what filter was selected
            SearchFilter selectedFilter = filter as SearchFilter;
            if (selectedFilter != null)
            {
                // Mirror the results into the corresponding Filter object to allow the
                // RadioButton representation used when not snapped to reflect the change
                selectedFilter.Active = true;

                switch (selectedFilter.Name)
                {
                    case "All":
                        this.ViewModel.Results = this.allNodes;
                        break;
                    case "Only Entries":
                        this.ViewModel.Results = this.allEntries;
                        break;
                    case "Only Groups":
                        this.ViewModel.Results = this.allGroups;
                        break;
                }

                if (this.ViewModel.Results != null && this.ViewModel.Results.Count > 0)
                {
                    VisualStateManager.GoToState(this, "ResultsFound", true);
                    return;
                }
            }

            // Display informational text when there are no search results.
            VisualStateManager.GoToState(this, "NoResultsFound", true);
        }
    }
}

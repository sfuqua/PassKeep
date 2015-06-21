using System;
using PassKeep.Lib.Contracts.Models;
using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using SariphLib.Infrastructure;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using PassKeep.Framework;

namespace PassKeep.Views
{
    /// <summary>
    /// A view over the active database, with a frame for displaying various child views.
    /// </summary>
    public sealed partial class DatabaseParentView : DatabaseParentViewBase
    {
        public DatabaseParentView()
            : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Provides access to the <see cref="Frame"/> that hosts database content.
        /// </summary>
        public override Frame ContentFrame
        {
            get { return this.databaseContentFrame; }
        }

        /// <summary>
        /// Handles navigating the content frame to the DatabaseView on first launch.
        /// </summary>
        /// <param name="e">EventArgs for the navigation.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.databaseContentFrame.Navigate(
                typeof(DatabaseView),
                this.ViewModel.GetDatabaseViewModel()
            );
        }

        /// <summary>
        /// EventHandler for user interaction with the BreadcrumbNavigator.
        /// </summary>
        /// <param name="sender">The BreadcrumbNavigator.</param>
        /// <param name="e">EventArgs provided the clicked group.</param>
        private void Breadcrumb_GroupClicked(object sender, GroupClickedEventArgs e)
        {
            IKeePassGroup clickedGroup = e.Group;
            Dbg.Assert(clickedGroup != null);

            IDatabaseChildView childView = this.databaseContentFrame.Content as IDatabaseChildView;
            Dbg.Assert(childView != null);

            childView.RequestBreadcrumbNavigation(
                this.ViewModel.GetDatabaseViewModel(),
                this.ViewModel.NavigationViewModel,
                clickedGroup
            );
        }
    }
}

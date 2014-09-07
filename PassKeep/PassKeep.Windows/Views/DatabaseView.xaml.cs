using PassKeep.Framework;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.EventArgClasses;
using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PassKeep.Views
{
    /// <summary>
    /// The primary View over the contents of a KeePass database.
    /// </summary>
    public sealed partial class DatabaseView : DatabaseViewBase
    {
        private const string EditResourceKey = "Edit";
        private const string DeleteResourceKey = "Delete";
        private const string CreateResourceKey = "Create";

        private AppBarButton editButton;
        private AppBarButton deleteButton;
        private AppBarButton createButton;

        public DatabaseView()
            : base()
        {
            this.InitializeComponent();

            this.editButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Edit),
                Label = GetString(DatabaseView.EditResourceKey),
                Command = null
            };

            this.deleteButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Delete),
                Label = GetString(DatabaseView.DeleteResourceKey),
                Command = null
            };

            this.createButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = GetString(DatabaseView.CreateResourceKey),
                Command = null
            };
        }

        /// <summary>
        /// Returns this View's primary CommandBar elements.
        /// </summary>
        /// <remarks>
        /// Edit, Delete, Create
        /// </remarks>
        /// <returns></returns>
        public override IList<ICommandBarElement> GetPrimaryCommandBarElements()
        {
            return new List<ICommandBarElement>
            {
                this.editButton,
                this.deleteButton,
                this.createButton
            };
        }

        public override IList<ICommandBarElement> GetSecondaryCommandBarElements()
        {
            return null;
        }

        #region Auto-event handles

        /// <summary>
        /// Auto-event handler for saving a database.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">CancellableEventArgs for the operation.</param>
        public void StartedSaveHandler(object sender, CancellableEventArgs e)
        {
            this.RaiseStartedLoading(
                new LoadingStartedEventArgs(
                    GetString(PassKeepPage.SavingResourceKey),
                    e.Cts
                )
            );
        }

        /// <summary>
        /// Auto-event handler for when a save operation has stopped.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">EventArgs for the notification.</param>
        public void StoppedSaveHandler(object sender, EventArgs e)
        {
            this.RaiseDoneLoading();
        }

        #endregion

        /// <summary>
        /// EventHandler for user interaction with the BreadcrumbNavigator.
        /// </summary>
        /// <param name="sender">The BreadcrumbNavigator.</param>
        /// <param name="e">EventArgs provided the clicked group.</param>
        private void Breadcrumb_GroupClicked(object sender, GroupClickedEventArgs e)
        {
            IKeePassGroup clickedGroup = e.Group;
            Debug.Assert(clickedGroup != null);

            Debug.WriteLine("Updating View to breadcrumb: {0}", e.Group.Title.ClearValue);
            this.ViewModel.NavigationViewModel.SetGroup(clickedGroup);
        }

        /// <summary>
        /// Handles updates to the "SortBy" ComboBox.
        /// </summary>
        /// <param name="sender">The sorting ComboBox.</param>
        /// <param name="e">EventArgs for the selection change.</param>
        private void Sort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            Debug.WriteLine("Handling sort selection change. New value: {0}", box.SelectedItem);
        }

        /// <summary>
        /// Handles queries from the SearchBox.
        /// </summary>
        /// <param name="sender">The querying SearchBox.</param>
        /// <param name="args">Args for the query.</param>
        private void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            Debug.WriteLine("Handling SearchBox query: {0}", args.QueryText);
            this.Frame.Navigate(
                typeof(SearchResultsView),
                new
                {
                    query = args.QueryText,
                    databaseViewModel = this.ViewModel
                }
            );
        }

        /// <summary>
        /// Handles clicks on the Child GridView.
        /// </summary>
        /// <param name="sender">The Child GridView.</param>
        /// <param name="e">ClickEventArgs for the action.</param>
        private void ChildGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            IKeePassGroup clickedGroup = e.ClickedItem as IKeePassGroup;
            if (clickedGroup != null)
            {
                this.ViewModel.NavigationViewModel.SetGroup(clickedGroup);
            }
        }
    }
}

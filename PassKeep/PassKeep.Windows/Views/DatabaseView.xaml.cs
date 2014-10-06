﻿using PassKeep.Converters;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.ViewModels;
using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

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

        private const string EditRenameResourceKey = "Rename";
        private const string EditDetailsResourceKey = "Details";

        private const string CreateEntryKey = "NewEntry";
        private const string CreateGroupKey = "NewGroup";

        private AppBarButton editButton;
        private AppBarButton deleteButton;
        private AppBarButton createButton;

        public DatabaseView()
            : base()
        {
            this.InitializeComponent();

            Binding selectedItemEnabledBinding = new Binding
            {
                Source = this.childGridView,
                Path = new PropertyPath("SelectedItem"),
                Converter = new ExistenceToBooleanConverter()
            };

            MenuFlyout editFlyout = new MenuFlyout();
            editFlyout.Items.Add(
                new MenuFlyoutItem { 
                    Text = GetString(DatabaseView.EditRenameResourceKey),
                    Command = null
                }
            );
            editFlyout.Items.Add(
                new MenuFlyoutItem {
                    Text = GetString(DatabaseView.EditDetailsResourceKey),
                    Command = null
                }
            );

            this.editButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Edit),
                Label = GetString(DatabaseView.EditResourceKey),
                Flyout = editFlyout
            };

            this.editButton.SetBinding(
                ButtonBase.IsEnabledProperty,
                selectedItemEnabledBinding
            );

            this.deleteButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Delete),
                Label = GetString(DatabaseView.DeleteResourceKey),
                Command = null
            };

            this.deleteButton.SetBinding(
                ButtonBase.IsEnabledProperty,
                selectedItemEnabledBinding
            );

            MenuFlyout createFlyout = new MenuFlyout();
            createFlyout.Items.Add(
                new MenuFlyoutItem {
                    Text = GetString(DatabaseView.CreateEntryKey),
                    Command = new ActionCommand(CreateEntry)
                }
            );
            createFlyout.Items.Add(
                new MenuFlyoutItem {
                    Text = GetString(DatabaseView.CreateGroupKey),
                    Command = new ActionCommand(CreateGroup)
                }
            );

            this.createButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = GetString(DatabaseView.CreateResourceKey),
                Flyout = createFlyout,
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

        #region Auto-event handlers

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
        /// Handles the user opting to create a new entry in the current group.
        /// </summary>
        private void CreateEntry()
        {

        }

        /// <summary>
        /// Handles the user opting to create a new group within the current group.
        /// </summary>
        private void CreateGroup()
        {

        }

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
                new NavigationParameter(
                    new {
                        query = args.QueryText,
                        databaseViewModel = this.ViewModel
                    }
                )
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
                // We clicked a group, so drill into it...
                this.ViewModel.NavigationViewModel.SetGroup(clickedGroup);
            }
            else
            {
                // The ClickedItem is assumed to be an entry if it is not a group.
                IKeePassEntry clickedEntry = e.ClickedItem as IKeePassEntry;
                Debug.Assert(clickedEntry != null);

                // For now, on item click, navigate to the EntryDetailsView.
                Frame.Navigate(
                    typeof(EntryDetailsView),
                    new NavigationParameter(
                        new {
                            persistenceService = this.ViewModel.PersistenceService,
                            navigationViewModel = this.ViewModel.NavigationViewModel,
                            document = this.ViewModel.Document,
                            entryToEdit = clickedEntry,
                            isReadOnly = true
                        },
                        ContainerHelper.EntryDetailsViewExisting
                    )
                );
            }
        }
    }
}

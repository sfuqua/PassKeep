using Microsoft.Practices.Unity;
using PassKeep.Common;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.ViewModels;
using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PassKeep.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class EntryDetailsView : EntryDetailsViewBase
    {
        public EntryDetailsView()
            : base()
        {
            this.InitializeComponent();
        }

        public override bool HandleAcceleratorKey(Windows.System.VirtualKey key)
        {
            // No accelerator key to handle
            return false;
        }

        protected override void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        protected override void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private void ConfirmedAction(Action callback)
        {
            if (true)
            {
                callback();
            }
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

            ConfirmedAction(
                () => {
                    Debug.WriteLine("Updating View to breadcrumb: {0}", e.Group.Title.ClearValue);
                    this.ViewModel.NavigationViewModel.SetGroup(clickedGroup);

                    Frame.Navigate(
                        typeof(DatabaseView),
                        new NavigationParameter(
                            new {
                                document = this.ViewModel.Document,
                                databasePersistenceService = this.ViewModel.PersistenceService,
                                navigationViewModel = this.ViewModel.NavigationViewModel
                            }
                        )
                    );
                }
            );
        }
    }
}

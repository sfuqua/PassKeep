using PassKeep.Lib.Contracts.Models;
using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace PassKeep.Views
{
    public sealed partial class DatabaseParentView : DatabaseParentViewBase
    {
        public DatabaseParentView()
            : base()
        {
            InitializeComponent();
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

            Dbg.Trace($"Updating View to breadcrumb: {e.Group.Title.ClearValue}");
            this.ViewModel.NavigationViewModel.SetGroup(clickedGroup);
        }
    }
}

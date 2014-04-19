using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PassKeep.ViewModels;
using PassKeep.Models;
using PassKeep.Common;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PassKeep.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class GroupDetailsView : GroupDetailsViewBase
    {
        public GroupDetailsView()
        {
            this.InitializeComponent();
        }

        protected override void navHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            base.navHelper_LoadState(sender, e);
            BreadcrumbViewModel.SetGroup(ViewModel.WorkingCopy);
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.WorkingCopy.Title.ClearValue = ((TextBox)sender).Text;
        }

        private void Notes_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.WorkingCopy.Notes.ClearValue = ((TextBox)sender).Text;
        }
    }
}

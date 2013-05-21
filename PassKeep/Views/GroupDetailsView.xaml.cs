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

        protected override void LoadState(object navigationParameter, Dictionary<string, object> pageState)
        {
            base.LoadState(navigationParameter, pageState);
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Item.Title.ClearValue = ((TextBox)sender).Text;
        }

        private void Notes_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Item.Notes.ClearValue = ((TextBox)sender).Text;
        }
    }
}

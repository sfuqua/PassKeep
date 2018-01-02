// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Framework;
using PassKeep.ViewBases;
using System;
using Windows.System;

namespace PassKeep.Views.FlyoutPages
{
    public partial class HelpView : HelpViewBase
    {
        public HelpView()
        {
            InitializeComponent();
        }

        private async void MailLink_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Uri mailUri = new Uri($"mailto:{this.contactEmail.Text}?subject=PassKeep Feedback");
            await Launcher.LaunchUriAsync(mailUri);
        }

        private async void SubredditLink_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Uri webUri = new Uri("https://www.reddit.com/r/PassKeep");
            await Launcher.LaunchUriAsync(webUri);
        }
    }
}

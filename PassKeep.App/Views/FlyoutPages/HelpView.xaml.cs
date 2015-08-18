using PassKeep.Framework;
using System;
using Windows.System;

namespace PassKeep.Views.FlyoutPages
{
    public partial class HelpView : PassKeepPage
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

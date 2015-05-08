using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PassKeep.Views.Controls
{
    /// <summary>
    /// An AppBarButton that toggles between enabling selection on a ListView
    /// and canceling back to IsItemClickEnabled.
    /// </summary>
    public sealed partial class SelectionModeAppBarButton : AppBarButton
    {
        /// <summary>
        /// The ListView controlled by this button.
        /// </summary>
        public static readonly DependencyProperty ListViewProperty
            = DependencyProperty.Register(
                nameof(ListView),
                typeof(ListViewBase),
                typeof(SelectionModeAppBarButton),
                PropertyMetadata.Create((object)null)
            );

        /// <summary>
        /// The selection mode to set on the ListView when in selection mode.
        /// </summary>
        public static readonly DependencyProperty SelectionModeProperty
            = DependencyProperty.Register(
                nameof(SelectionMode),
                typeof(ListViewSelectionMode),
                typeof(SelectionModeAppBarButton),
                PropertyMetadata.Create(ListViewSelectionMode.Single)
            );

        /// <summary>
        /// Whether the button currently indicates the ListView is in selection mode.
        /// </summary>
        public static readonly DependencyProperty IsSelectionModeProperty
            = DependencyProperty.Register(
                nameof(IsSelectionMode),
                typeof(bool),
                typeof(SelectionModeAppBarButton),
                PropertyMetadata.Create(false)
            );

        public static readonly string SelectGlyph = "\uE762";
        public static readonly string CancelGlyph = "\uE894";

        public readonly string SelectLabel;
        public readonly string CancelLabel;

        /// <summary>
        /// Fetches localized labels and sets the initial state of the control.
        /// </summary>
        public SelectionModeAppBarButton()
        {
            this.InitializeComponent();
            this.IsSelectionMode = false;

            ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

            SelectLabel = resourceLoader.GetString("Select");
            CancelLabel = resourceLoader.GetString("Cancel");

            this.Icon = new FontIcon
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets")
            };
            SyncState();
        }

        /// <summary>
        /// The ListView controlled by this button.
        /// </summary>
        public ListViewBase ListView
        {
            get { return (ListViewBase)GetValue(ListViewProperty); }
            set { SetValue(ListViewProperty, value); }
        }

        /// <summary>
        /// The selection mode to set on the ListView when in selection mode.
        /// </summary>
        public ListViewSelectionMode SelectionMode
        {
            get { return (ListViewSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        /// <summary>
        /// Whether the button currently indicates the ListView is in selection mode.
        /// </summary>
        public bool IsSelectionMode
        {
            get { return (bool)GetValue(IsSelectionModeProperty); }
            set { SetValue(IsSelectionModeProperty, value); }
        }

        /// <summary>
        /// Syncs the state of the AppBarButton with the internal toggle state.
        /// </summary>
        private void SyncState()
        {
            if (this.IsSelectionMode)
            {
                this.Label = CancelLabel;
                ((FontIcon)this.Icon).Glyph = CancelGlyph;

                if (this.ListView != null)
                {
                    this.ListView.IsItemClickEnabled = false;
                    this.ListView.SelectionMode = this.SelectionMode;
                }
            }
            else
            {
                this.Label = SelectLabel;
                ((FontIcon)this.Icon).Glyph = SelectGlyph;

                if (this.ListView != null)
                {
                    this.ListView.IsItemClickEnabled = true;
                    this.ListView.SelectionMode = ListViewSelectionMode.None;
                }
            }
        }

        /// <summary>
        /// Handles clicks on the button.
        /// </summary>
        /// <param name="sender">This button.</param>
        /// <param name="e"></param>
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsSelectionMode = !this.IsSelectionMode;
            SyncState();
        }
    }
}

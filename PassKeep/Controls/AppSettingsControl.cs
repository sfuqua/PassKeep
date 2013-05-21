using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace PassKeep.Controls
{
    public abstract class AppSettingsControl : ContentControl
    {
        public event EventHandler BackPressed;

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(AppSettingsControl),
                PropertyMetadata.Create("Default")
            );
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty PaneTransitionsProperty =
            DependencyProperty.Register(
                "PaneTransitions",
                typeof(TransitionCollection),
                typeof(AppSettingsControl),
                PropertyMetadata.Create(new TransitionCollection())
            );
        public TransitionCollection PaneTransitions
        {
            get { return (TransitionCollection)GetValue(PaneTransitionsProperty); }
            set { SetValue(TransitionsProperty, value); }
        }

        public AppSettingsControl()
        {
            this.DefaultStyleKey = typeof(AppSettingsControl);
            DataContext = this;   
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var backButton = GetTemplateChild("PART_backButton") as Button;
            if (backButton != null)
            {
                backButton.Click += (s, e) =>
                    {
                        if (BackPressed != null)
                        {
                            BackPressed(this, new EventArgs());
                        }
                    };
            }

            var layout = GetTemplateChild("PART_layoutRoot") as Panel;
            layout.Height = Window.Current.Bounds.Height;

            var popup = GetTemplateChild("PART_popup") as Popup;
            if (popup != null)
            {
                popup.IsOpen = true;
            }
        }
    }
}

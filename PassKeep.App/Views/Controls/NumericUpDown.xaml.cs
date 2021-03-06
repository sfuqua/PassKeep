﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Diagnostics;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PassKeep.Views.Controls
{
    public sealed partial class NumericUpDown : UserControl
    {
        /// <summary>
        /// The current value of the spinner.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(int),
            typeof(NumericUpDown),
            PropertyMetadata.Create(0)
        );

        /// <summary>
        /// The minimum value of the spinner.
        /// </summary>
        public static readonly DependencyProperty MinProperty = DependencyProperty.Register(
            "Min",
            typeof(int),
            typeof(NumericUpDown),
            PropertyMetadata.Create(0)
        );

        /// <summary>
        /// The maximum value of the spinner.
        /// </summary>
        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register(
            "Max",
            typeof(int),
            typeof(NumericUpDown),
            PropertyMetadata.Create(int.MaxValue)
        );

        /// <summary>
        /// Helper property for the current value of the spinner.
        /// </summary>
        public int Value
        {
            get { return Convert.ToInt32(GetValue(ValueProperty)); }
            set
            {
                // Clamp the new value to [min, max]
                value = Math.Max(Math.Min(value, Max), Min);
                SetValue(ValueProperty, value);
                this.Bindings.Update();
            }
        }

        /// <summary>
        /// Helper property for the minimum value of the spinner.
        /// </summary>
        public int Min
        {
            get { return (int)GetValue(MinProperty); }
            set
            {
                if (value > Max)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (value > Value)
                {
                    Value = value;
                }

                SetValue(MinProperty, value);
            }
        }

        /// <summary>
        /// Helper property for the max value of the spinner.
        /// </summary>
        public int Max
        {
            get { return (int)GetValue(MaxProperty); }
            set
            {
                if (value < Min)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (value < Value)
                {
                    Value = value;
                }

                SetValue(MaxProperty, value);
            }
        }

        public NumericUpDown()
        {
            InitializeComponent();
        }

        private void downButton_Click(object sender, RoutedEventArgs e)
        {
            Value -= 1;
        }

        private void upButton_Click(object sender, RoutedEventArgs e)
        {
            Value += 1;
        }

        /// <summary>
        /// Handles validating and updating the control properties when the user changes the value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            DebugHelper.Assert(sender == this.textbox);
            TextBox box = (TextBox)sender;

            if (Int32.TryParse(box.Text, out int newValue))
            {
                Value = newValue;
            }
            else
            {
                // Invalid input from user, abort by default to min.
                box.Text = Value.ToString();
            }
        }
    }
}

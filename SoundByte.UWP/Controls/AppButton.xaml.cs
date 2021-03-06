﻿using Windows.UI.Xaml;

namespace SoundByte.UWP.Controls
{
    public sealed partial class AppButton
    {
        public delegate void ClickEventHandler(object sender, RoutedEventArgs e);

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(AppButton), null);

        public static readonly DependencyProperty GlyphProperty =
            DependencyProperty.Register("Glyph", typeof(string), typeof(AppButton), null);

        public static readonly DependencyProperty IsExtendedProperty =
            DependencyProperty.Register("IsExtended", typeof(bool), typeof(AppButton), null);

        //ButtonText

        public AppButton()
        {
            InitializeComponent();

            MainButton.Click += (sender, args) => { Click?.Invoke(sender, args); };
        }

        /// <summary>
        /// Should more text be shown
        /// </summary>
        public bool IsExtended
        {
            get => (bool) GetValue(IsExtendedProperty);
            set
            {
                SetValue(IsExtendedProperty, value);

                ButtonText.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        ///     The label to show on the button
        /// </summary>
        public string Label
        {
            get => GetValue(LabelProperty) as string;
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        ///     The glyph to show on the button
        /// </summary>
        public string Glyph
        {
            get => GetValue(GlyphProperty) as string;
            set => SetValue(GlyphProperty, value);
        }

        /// <summary>
        ///     Handles the button click event
        /// </summary>
        public event ClickEventHandler Click;
    }
}
﻿/* |----------------------------------------------------------------|
 * | Copyright (c) 2017, Grid Entertainment                         |
 * | All Rights Reserved                                            |
 * |                                                                |
 * | This source code is to only be used for educational            |
 * | purposes. Distribution of SoundByte source code in             |
 * | any form outside this repository is forbidden. If you          |
 * | would like to contribute to the SoundByte source code, you     |
 * | are welcome.                                                   |
 * |----------------------------------------------------------------|
 */

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Microsoft.Toolkit.Uwp.UI.Animations;
using SoundByte.Core.Items.Playlist;
using UICompositionAnimations.Composition;

namespace SoundByte.UWP.Controls
{
    public sealed partial class PlaylistItem 
    {
        /// <summary>
        /// Identifies the Playlist dependency property.
        /// </summary>
        public static readonly DependencyProperty PlaylistProperty =
            DependencyProperty.Register(nameof(Playlist), typeof(BasePlaylist), typeof(PlaylistItem), null);

        /// <summary>
        /// Gets or sets the rounding interval for the Value.
        /// </summary>
        public BasePlaylist Playlist
        {
            get => (BasePlaylist)GetValue(PlaylistProperty);
            set => SetValue(PlaylistProperty, value);
        }

        public PlaylistItem()
        {
            InitializeComponent();
        }

        private async void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            FindName("HoverArea");

            ShadowPanel.DropShadow.StartAnimation("Offset.Y",
                ShadowPanel.DropShadow.Compositor.CreateScalarKeyFrameAnimation(null, 10.0f, TimeSpan.FromMilliseconds(250), null));

            ShadowPanel.DropShadow.StartAnimation("Opacity",
                ShadowPanel.DropShadow.Compositor.CreateScalarKeyFrameAnimation(null, 0.9f, TimeSpan.FromMilliseconds(200), null));

            ShadowPanel.DropShadow.StartAnimation("BlurRadius",
                ShadowPanel.DropShadow.Compositor.CreateScalarKeyFrameAnimation(null, 45.0f, TimeSpan.FromMilliseconds(200), null));

            PlaylistImage.Blur(5, 200).Start();
            await HoverArea.Fade(1, 200).StartAsync();
        }

        private async void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            ShadowPanel.DropShadow.StartAnimation("Offset.Y",
                ShadowPanel.DropShadow.Compositor.CreateScalarKeyFrameAnimation(null, 6.0f, TimeSpan.FromMilliseconds(250), null));

            ShadowPanel.DropShadow.StartAnimation("Opacity",
                ShadowPanel.DropShadow.Compositor.CreateScalarKeyFrameAnimation(null, 0.4f, TimeSpan.FromMilliseconds(200), null));

            ShadowPanel.DropShadow.StartAnimation("BlurRadius",
                ShadowPanel.DropShadow.Compositor.CreateScalarKeyFrameAnimation(null, 25.0f, TimeSpan.FromMilliseconds(200), null));

            var colorAnimation = ShadowPanel.DropShadow.Compositor.CreateColorKeyFrameAnimation();
            colorAnimation.Duration = TimeSpan.FromMilliseconds(200);

            PlaylistImage.Blur(0, 200).Start();

            if (HoverArea != null)
                await HoverArea.Fade(0, 200).StartAsync();

            UnloadObject(HoverArea);
        }
    }
}

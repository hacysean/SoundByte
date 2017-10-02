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
using SoundByte.Core.Items.Playlist;
using SoundByte.Core.Items.Track;
using SoundByte.UWP.Dialogs;
using SoundByte.UWP.Helpers;
using SoundByte.UWP.Services;

namespace SoundByte.UWP.UserControls
{
    /// <summary>
    ///     Base item for users, tracks, reposted tracks, playlists, reposted playlists and groups
    /// </summary>
    public sealed partial class SoundByteStreamItem
    {
        public SoundByteStreamItem()
        {
            // Load the xaml
            InitializeComponent();
            // Setup the even that is called when the data
            // context chanages.
            DataContextChanged += delegate
            {
                // Switch through all the items
                switch (TrackType)
                {
                    case "track-repost":
                    case "track":
                        VisualStateManager.GoToState(this, "TrackItem", false);

                        if (DeviceHelper.IsXbox || DeviceHelper.IsMobile)
                            TrackExtendedDetailPane.Visibility = Visibility.Collapsed;

                        break;
                    case "playlist-repost":
                    case "playlist":
                        VisualStateManager.GoToState(this, "PlaylistItem", false);


                        if (DeviceHelper.IsXbox || DeviceHelper.IsMobile)
                            PlaylistExtendedDetailPane.Visibility = Visibility.Collapsed;

                        break;
                }
            };
        }

        public async void ShareTrack()
        {
            await NavigationService.Current.CallDialogAsync<ShareDialog>(Track.ToBaseTrack());
        }

        public async void AddTrackToPlaylist()
        {
            await NavigationService.Current.CallDialogAsync<PlaylistDialog>(Track.ToBaseTrack());
        }

        #region Variables

        // What type of track this is
        public static readonly DependencyProperty TrackTypeProperty =
            DependencyProperty.Register("TrackType", typeof(string), typeof(SoundByteStreamItem), null);

        // When this was created
        public static readonly DependencyProperty CreatedProperty =
            DependencyProperty.Register("Created", typeof(string), typeof(SoundByteStreamItem), null);

        // The track object
        public static readonly DependencyProperty TrackProperty =
            DependencyProperty.Register("Track", typeof(SoundCloudTrack), typeof(SoundByteStreamItem), null);

        // The playlist object
        public static readonly DependencyProperty PlaylistProperty =
            DependencyProperty.Register("Playlist", typeof(SoundCloudPlaylist), typeof(SoundByteStreamItem), null);

        #endregion

        #region Getters and Setters

        /// <summary>
        ///     What is the current type of track (e.g repost, playlist, etc.)
        /// </summary>
        public string TrackType
        {
            get => GetValue(TrackTypeProperty) as string;
            set => SetValue(TrackTypeProperty, value);
        }

        /// <summary>
        ///     When the track was created
        /// </summary>
        public string Created
        {
            get => GetValue(CreatedProperty) as string;
            set => SetValue(CreatedProperty, value);
        }

        /// <summary>
        ///     The track object
        /// </summary>
        public SoundCloudTrack Track
        {
            get => GetValue(TrackProperty) as SoundCloudTrack;
            set => SetValue(TrackProperty, value);
        }

        /// <summary>
        ///     The track object
        /// </summary>
        public SoundCloudPlaylist Playlist
        {
            get => GetValue(PlaylistProperty) as SoundCloudPlaylist;
            set => SetValue(PlaylistProperty, value);
        }

        #endregion
    }
}
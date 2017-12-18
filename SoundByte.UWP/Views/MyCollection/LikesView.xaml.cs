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
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using SoundByte.Core;
using SoundByte.Core.Items.Track;
using SoundByte.Core.Services;
using SoundByte.Core.Sources.SoundCloud;
using SoundByte.UWP.Helpers;
using SoundByte.UWP.Services;
using SoundByte.UWP.ViewModels;
using SoundByte.UWP.ViewModels.Generic;
using SoundByte.UWP.Views.Generic;

namespace SoundByte.UWP.Views.MyCollection
{
    public sealed partial class LikesView
    {
        public SoundByteCollection<LikeSoundCloudSource, BaseTrack> SoundCloudLikes { get; } =
            new SoundByteCollection<LikeSoundCloudSource, BaseTrack>();

        public LikesView()
        {
            InitializeComponent();

            SoundCloudLikes.Source.User = SoundByteV3Service.Current.GetConnectedUser(ServiceType.SoundCloud);
        }

        public async void PlaySoundCloudLikes(object sender, ItemClickEventArgs e)
        {
            var startPlayback = await PlaybackService.Instance.StartModelMediaPlaybackAsync(SoundCloudLikes, false, (BaseTrack)e.ClickedItem);
            if (!startPlayback.Success)
                await new MessageDialog(startPlayback.Message, "Error playing track.").ShowAsync();
        }


        public void NavigateMoreSoundCloudLikes()
        {
            App.NavigateTo(typeof(TrackListView), new TrackListViewModel.TrackViewModelHolder
            {
                Track = SoundCloudLikes.Source,
                Title = "Likes",
                Subtitle = "Likes"
            });
        }

        public async void PlayShuffleSoundCloud()
        {
            await BaseViewModel.ShuffleTracksAsync(SoundCloudLikes);
        }

        public async void PlaySoundCloud()
        {
            await BaseViewModel.PlayAllItemsAsync(SoundCloudLikes);
        }
    }
}
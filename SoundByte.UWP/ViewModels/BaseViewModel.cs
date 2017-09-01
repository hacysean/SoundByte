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
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Popups;
using SoundByte.API.Items.Track;
using SoundByte.UWP.Services;

namespace SoundByte.UWP.ViewModels
{
    /// <summary>
    ///     Base class for all view models to extend off of
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        ///     The global playback service
        /// </summary>
        public PlaybackService Service => PlaybackService.Instance;

        /// <summary>
        ///     Dispose the model
        /// </summary>
        public virtual void Dispose()
        {
            // Collect memory if in background
         //   if (DeviceHelper.IsBackground)
         //   {
                GC.Collect();
         //   }
        }

        /// <summary>
        ///     Performs a shuffle of the tracks
        /// </summary>
        /// <param name="tracks"></param>
        /// <param name="token"></param>
        public static async Task ShuffleTracksAsync(List<BaseTrack> tracks, string token)
        {
            App.IsLoading = true;

            var startPlayback = await PlaybackService.Instance.StartMediaPlayback(tracks, token, true);

            if (!startPlayback.success)
                await new MessageDialog(startPlayback.message, "Error playing shuffled tracks.").ShowAsync();

            App.IsLoading = false;
        }

        #region Property Changed Event Handlers

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        protected void UpdateProperty([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
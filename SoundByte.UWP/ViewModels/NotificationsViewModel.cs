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
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SoundByte.API.Endpoints;
using SoundByte.API.Exceptions;
using SoundByte.API.Items.Track;
using SoundByte.UWP.Models;
using SoundByte.UWP.Services;
using SoundByte.UWP.Views;

namespace SoundByte.UWP.ViewModels
{
    /// <summary>
    ///     View model for the notifications page
    /// </summary>
    public class NotificationsViewModel : BaseViewModel
    {
        private NotificationModel _notificationItems;

        public NotificationsViewModel()
        {
            NotificationItems = new NotificationModel();
        }

        public NotificationModel NotificationItems
        {
            get => _notificationItems;
            private set
            {
                if (value == _notificationItems) return;

                _notificationItems = value;
                UpdateProperty();
            }
        }

        public override void Dispose()
        {
            _notificationItems.Clear();
            _notificationItems = null;

            GC.Collect();
        }

        #region Binding Methods

        /// <summary>
        ///     Reloads any notifications on the page
        /// </summary>
        public void RefreshPage()
        {
            // Show the loading ring as this task can take a while
            App.IsLoading = true;

            // Reload the items
            NotificationItems.RefreshItems();

            // Hide the loading ring as we are done
            App.IsLoading = false;
        }

        /// <summary>
        ///     Navigates to a notification
        /// </summary>
        public async void NotificationNavigate(object sender, ItemClickEventArgs e)
        {
            // Show the loading ring as this process can take a while
            App.IsLoading = true;
            // Get the notification collection
            var notification = (Notification) e.ClickedItem;
            // Switch between the notification types
            try
            {
                switch (notification.Type)
                {
                    case "track-like":
                        // Play this item
                        var startPlayback =
                            await PlaybackService.Instance.StartMediaPlayback(new List<BaseTrack> {notification.Track.ToBaseTrack()},
                                $"Notification-{notification.Track.Id}");
                        if (!startPlayback.success)
                            await new MessageDialog(startPlayback.message, "Error opening Notification.").ShowAsync();
                        break;
                    case "comment":
                        // Play this item
                        var startPlaybackComment =
                            await PlaybackService.Instance.StartMediaPlayback(
                                new List<BaseTrack> {notification.Comment.Track.ToBaseTrack()},
                                $"Notification-{notification.Comment.Track.Id}");
                        if (!startPlaybackComment.success)
                            await new MessageDialog(startPlaybackComment.message, "Error opening Notification.")
                                .ShowAsync();
                        break;
                    case "affiliation":
                        // Navigate to the user page
                        App.NavigateTo(typeof(UserView), notification.User);
                        break;
                }
            }
            catch (SoundByteException ex)
            {
                // Get the resource loader
                var resources = ResourceLoader.GetForCurrentView();
                // Create and display the error dialog
                await new ContentDialog
                {
                    Title = ex.ErrorTitle,
                    Content = new TextBlock {TextWrapping = TextWrapping.Wrap, Text = ex.ErrorDescription},
                    IsPrimaryButtonEnabled = true,
                    PrimaryButtonText = resources.GetString("Close_Button")
                }.ShowAsync();
            }
            // Hide the loading ring now that data has been loaded and displayed
            App.IsLoading = false;
        }

        #endregion
    }
}
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Data;
using Microsoft.Toolkit.Uwp.Helpers;
using SoundByte.Core;
using SoundByte.Core.Exceptions;
using SoundByte.Core.Holders;
using SoundByte.Core.Items.User;
using SoundByte.Core.Services;
using SoundByte.UWP.Services;

namespace SoundByte.UWP.Models
{
    public class UserFollowersModel : ObservableCollection<BaseUser>, ISupportIncrementalLoading
    {
        public UserFollowersModel(BaseUser user, string type)
        {
            User = user;
            Type = type;
        }

        /// <summary>
        ///     User object that we will used to get the follower / followings for
        /// </summary>
        public BaseUser User { get; set; }

        /// <summary>
        ///     What type of object is this (followers, followings)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///     The position of the track, will be 'eol'
        ///     if there are no new tracks
        /// </summary>
        public string Token { get; protected set; }

        /// <summary>
        ///     Are there more items to load
        /// </summary>
        public bool HasMoreItems => Token != "eol";

        /// <summary>
        ///     Loads stream items from the souncloud api
        /// </summary>
        /// <param name="count">The amount of items to load</param>
        // ReSharper disable once RedundantAssignment
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            // Return a task that will get the items
            return Task.Run(async () =>
            {
                // We are loading
                await DispatcherHelper.ExecuteOnUIThreadAsync(() => { App.IsLoading = true; });

                // Get the resource loader
                var resources = ResourceLoader.GetForViewIndependentUse();

                try
                {
                    // Get the users playlists using the V2 API
                    var userPlaylists = await SoundByteV3Service.Current.GetAsync<UserListHolder>(ServiceType.SoundCloud, 
                        $"/users/{User.Id}/{Type}", new Dictionary<string, string>
                        {
                            {"limit", "50"},
                            {"cursor", Token},
                            {"linked_partitioning", "1"}
                        });

                    // Parse uri for offset
                    var param = new QueryParameterCollection(userPlaylists.NextList);
                    var offset = param.FirstOrDefault(x => x.Key == "cursor").Value;

                    // Get the playlist cursor
                    Token = string.IsNullOrEmpty(offset) ? "eol" : offset;

                    // Make sure that there are tracks in the list
                    if (userPlaylists.Users.Count > 0)
                    {
                        // Set the count variable
                        count = (uint) userPlaylists.Users.Count;

                        // Loop though all the playlists on the UI thread
                        await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                        {
                            foreach (var user in userPlaylists.Users)
                            {
                                Add(user.ToBaseUser());
                            }
                        });
                    }
                    else
                    {
                        // There are no items, so we added no items
                        count = 0;

                        // Reset the token
                        Token = "eol";

                        // No items tell the user
                        await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                        {
                            await new MessageDialog(resources.GetString("UserPlaylists_Content"),
                                resources.GetString("UserPlaylists_Header")).ShowAsync();
                        });
                    }
                }
                catch (SoundByteException ex)
                {
                    // Exception, most likely did not add any new items
                    count = 0;

                    // Exception, display error to the user
                    await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                    {
                        await new MessageDialog(ex.ErrorDescription, ex.ErrorTitle).ShowAsync();
                    });
                }

                // We are not loading
                await DispatcherHelper.ExecuteOnUIThreadAsync(() => { App.IsLoading = false; });

                // Return the result
                return new LoadMoreItemsResult {Count = count};
            }).AsAsyncOperation();
        }

        /// <summary>
        ///     Refresh the list by removing any
        ///     existing items and reseting the token.
        /// </summary>
        public void RefreshItems()
        {
            Token = null;
            Clear();
        }
    }
}
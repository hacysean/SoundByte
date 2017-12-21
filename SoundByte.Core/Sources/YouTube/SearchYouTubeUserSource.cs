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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SoundByte.Core.Items.User;
using SoundByte.Core.Items.YouTube;
using SoundByte.Core.Services;

namespace SoundByte.Core.Sources.YouTube
{
    [UsedImplicitly]
    public class SearchYouTubeUserSource : ISource<BaseUser>
    {
        /// <summary>
        /// What we should search for
        /// </summary>
        public string SearchQuery { get; set; }

        public async Task<SourceResponse<BaseUser>> GetItemsAsync(int count, string token,
            CancellationTokenSource cancellationToken = default(CancellationTokenSource))
        {
            // Call the YouTube API and get the items
            var users = await SoundByteV3Service.Current.GetAsync<YouTubeChannelHolder>(ServiceType.YouTube,
                "search",
                new Dictionary<string, string>
                {
                    {"part", "snippet"},
                    {"maxResults", count.ToString()},
                    {"pageToken", token},
                    {"type", "channel"},
                    {"q", WebUtility.UrlEncode(SearchQuery)}
                }, cancellationToken).ConfigureAwait(false);


            // If there are no users
            if (!users.Channels.Any())
            {
                return new SourceResponse<BaseUser>(null, null, false, "No results found",
                    "Could not find any results for '" + SearchQuery + "'");
            }

            // Convert YouTube specific channels to base users
            var baseUsers = new List<BaseUser>();
            foreach (var user in users.Channels)
            {
                if (user.Id.Kind == "youtube#channel")
                {
                    baseUsers.Add(user.ToBaseUser());
                }
            }

            // Return the items
            return new SourceResponse<BaseUser>(baseUsers, users.NextList);
        }
    }
}

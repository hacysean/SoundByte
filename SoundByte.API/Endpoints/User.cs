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

using Newtonsoft.Json;

namespace SoundByte.API.Endpoints
{
    /// <summary>
    ///     A Soundcloud user
    /// </summary>
    [JsonObject]
    public class User
    {
        /// <summary>
        ///     The ID of the user
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        ///     The users username
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        ///     Gets the url of the users picture
        /// </summary>
        [JsonProperty("avatar_url")]
        public string ArtworkLink { get; set; }

        /// <summary>
        ///     The country that the user is from
        /// </summary>
        [JsonProperty("country")]
        public string Country { get; set; }

        /// <summary>
        ///     A link to this users profile
        /// </summary>
        [JsonProperty("permalink_url")]
        public string PermalinkUri { get; set; }

        /// <summary>
        ///     About the user
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        ///     The number of tracks the user has uploaded
        /// </summary>
        [JsonProperty("track_count")]
        public double? TrackCount { get; set; }

        /// <summary>
        ///     The amount of playlists the user owns
        /// </summary>
        [JsonProperty("playlist_count")]
        public double? PlaylistCount { get; set; }

        /// <summary>
        ///     The amount of followers this user has
        /// </summary>
        [JsonProperty("followers_count")]
        public double? FollowersCount { get; set; }

        /// <summary>
        ///     The amount of followings this user has
        /// </summary>
        [JsonProperty("followings_count")]
        public double? FollowingsCount { get; set; }
    }
}
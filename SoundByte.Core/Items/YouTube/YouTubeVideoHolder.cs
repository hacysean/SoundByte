﻿/* |----------------------------------------------------------------|
 * | Copyright (c) 2017 - 2018 Grid Entertainment                   |
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
using Newtonsoft.Json;
using SoundByte.Core.Items.Track;

namespace SoundByte.Core.Items.YouTube
{
    [JsonObject]
    public class YouTubeVideoHolder
    {
        /// <summary>
        ///     Collection of tracks
        /// </summary>
        [JsonProperty("items")]
        public List<YouTubeTrack> Tracks { get; set; }

        /// <summary>
        ///     The next list of items
        /// </summary>
        [JsonProperty("nextPageToken")]
        public string NextList { get; set; }
    }
}

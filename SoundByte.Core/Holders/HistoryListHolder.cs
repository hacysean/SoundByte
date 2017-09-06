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
using Newtonsoft.Json;
using SoundByte.Core.Items.Track;

namespace SoundByte.Core.Holders
{
    /// <summary>
    ///     Holds a track
    /// </summary>
    [JsonObject]
    [Obsolete]
    public class HistoryBootstrap
    {
        /// <summary>
        ///     A playlist
        /// </summary>
        [JsonProperty("track")]
        public SoundCloudTrack Track { get; set; }
    }

    [JsonObject]
    [Obsolete]
    public class HistoryListHolder
    {
        [JsonProperty("collection")]
        public List<HistoryBootstrap> Tracks { get; set; }

        [JsonProperty("next_href")]
        public string NextList { get; set; }
    }
}
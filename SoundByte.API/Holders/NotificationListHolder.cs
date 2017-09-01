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
using SoundByte.API.Endpoints;

namespace SoundByte.API.Holders
{
    /// <summary>
    ///     Holds a list of notifications
    /// </summary>
    [JsonObject]
    [Obsolete]
    public class NotificationListHolder
    {
        /// <summary>
        ///     A list of notification items
        /// </summary>
        [JsonProperty("collection")]
        public List<Notification> Notifications { get; set; }

        /// <summary>
        ///     Link to the next list of items
        /// </summary>
        [JsonProperty("next_href")]
        public string NextList { get; set; }
    }
}
﻿//*********************************************************
/* |----------------------------------------------------------------|
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
using SoundByte.Core.Endpoints;

namespace SoundByte.Core.Holders
{
    /// <summary>
    ///     Small class for holding comments
    /// </summary>
    [JsonObject]
    [Obsolete]
    public class CommentListHolder
    {
        /// <summary>
        ///     List of comments
        /// </summary>
        [JsonProperty("collection")]
        public List<Comment> Items { get; set; }

        /// <summary>
        ///     Next items in the list
        /// </summary>
        [JsonProperty("next_href")]
        public string NextList { get; set; }
    }
}
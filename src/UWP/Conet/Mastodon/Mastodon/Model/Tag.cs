﻿using Newtonsoft.Json;

namespace Mastodon.Model
{
    public class Tag
    {
        /// <summary>
        ///     The hashtag, not including the preceding #
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        ///     The URL of the hashtag
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
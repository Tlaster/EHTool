﻿using Newtonsoft.Json;

namespace Conet.Apis.Mastodon.Model
{
    public class CardModel
    {
        /// <summary>
        /// The url associated with the card
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// The title of the card
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// The card description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// The image associated with the card, if any
        /// </summary>
        [JsonProperty("image")]
        public string Image { get; set; }
    }
}

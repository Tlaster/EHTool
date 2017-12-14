﻿using Newtonsoft.Json;

namespace iHentai.Apis.NHentai.Models
{
    public class TitleModel
    {
        [JsonProperty("japanese")]
        public string Japanese { get; set; }

        [JsonProperty("pretty")]
        public string Pretty { get; set; }

        [JsonProperty("english")]
        public string English { get; set; }
    }
}
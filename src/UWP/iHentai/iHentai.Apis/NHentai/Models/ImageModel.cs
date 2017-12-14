﻿using Newtonsoft.Json;

namespace iHentai.Apis.NHentai.Models
{
    public class ImageModel
    {
        [JsonProperty("h")]
        public int Height { get; set; }

        [JsonProperty("t")]
        public string Type { get; set; }

        [JsonProperty("w")]
        public int Width { get; set; }
    }
}
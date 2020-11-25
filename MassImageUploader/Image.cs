using Newtonsoft.Json;
using System;

namespace MassImageUploader
{
    public class Image
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("copyright")]
        public string Copyright { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("explanation")]

        public string Explanation { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Whiteboard
    {
        [JsonProperty("wid")]
        public int WId { get; set; }
        [JsonProperty("t")]
        public string Tool { get; set; }
        [JsonProperty("d")]
        public List<object> Distance { get; set; }
        [JsonProperty("c")]
        public string Color { get; set; }
        [JsonProperty("th")]
        public float Thickness { get; set; }
        [JsonProperty("username")]
        public string UserId { get; set; }
        [JsonProperty("drawId")]
        public int DrawId { get; set; }
        [JsonProperty("event")]
        public string Event { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("draw")]
        public string Draw { get; set; }
    }
}

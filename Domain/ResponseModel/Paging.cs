using Newtonsoft.Json;

namespace Domain
{
    public class Paging
    {
        [JsonProperty("offset")]
        public int Offset { get; set; }        
        [JsonProperty("limit")]
        public int Limit { get; set; }        
        [JsonProperty("total")]
        public int Total { get; set; }
    }
}

using Newtonsoft.Json;

namespace Domain.ResponseModel
{
    public class PagingModel
    {
        [JsonProperty("offset")]
        public int Offset { get; set; }      
        [JsonProperty("limit")]
        public int Limit { get; set; }    
        [JsonProperty("total")]
        public int Total { get; set; }
    }
}

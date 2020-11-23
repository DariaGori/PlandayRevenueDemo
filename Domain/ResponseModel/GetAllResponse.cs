using Newtonsoft.Json;
using System.Collections.Generic;

namespace Domain
{
    public class GetAllResponse<T>
    {
        [JsonProperty("paging")]
        public Paging Paging { get; set; }
        [JsonProperty("data")]
        public List<T> DataUnits { get; set; }
    }
}

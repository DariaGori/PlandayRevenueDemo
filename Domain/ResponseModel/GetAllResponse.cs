using Newtonsoft.Json;
using System.Collections.Generic;
using Domain.Contract;

namespace Domain
{
    public class GetAllResponse<T>
    {
        [JsonProperty("paging")]
        public Paging Paging { get; set; } = default!;
        [JsonProperty("data")]
        public List<T>? DataUnits { get; set; }
    }
}

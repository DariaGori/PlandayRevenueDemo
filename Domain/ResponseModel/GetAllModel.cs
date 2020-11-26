using System.Collections.Generic;
using Newtonsoft.Json;

namespace Domain.ResponseModel
{
    public class GetAllModel<T>
    {
        [JsonProperty("paging")]
        public PagingModel Paging { get; set; } = default!;
        [JsonProperty("data")]
        public List<T>? DataUnits { get; set; }
    }
}

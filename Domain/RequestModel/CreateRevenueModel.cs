using System;
using Newtonsoft.Json;

namespace Domain.RequestModel
{
    public class CreateRevenueModel
    {
        [JsonProperty("description")]
        public String Description { get; set; } = default!;
        [JsonProperty("turnover")]
        public Decimal Turnover { get; set; }
        [JsonProperty("revenueUnitId")]
        public int RevenueUnitId { get; set; }
        [JsonProperty("date")]
        public String Date { get; set; } = default!;
    }
}

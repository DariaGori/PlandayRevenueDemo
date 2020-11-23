using Newtonsoft.Json;
using System;

namespace Domain
{
    public class CreateRevenueRequestDto
    {
        [JsonProperty("description")]
        public String Description { get; set; }
        [JsonProperty("turnover")]
        public Double Turnover { get; set; }
        [JsonProperty("revenueUnitId")]
        public int RevenueUnitId { get; set; }
        [JsonProperty("date")]
        public String Date { get; set; }
    }
}

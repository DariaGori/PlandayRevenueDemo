using Newtonsoft.Json;

namespace Domain
{
    public class Revenue
    {
        [JsonProperty("dayScheduleId")]
        public int DayScheduleId { get; set; }
        [JsonProperty("revenueUnitId")]
        public int RevenueUnitId { get; set; }
    }
}

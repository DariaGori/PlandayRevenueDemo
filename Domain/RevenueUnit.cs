using Newtonsoft.Json;
using System;

namespace Domain
{
    public class RevenueUnit
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public String Name { get; set; }
        [JsonProperty("departmentId")]
        public int DepartmentId { get; set; }
    }
}

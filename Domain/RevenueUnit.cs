using Newtonsoft.Json;
using System;
using Domain.Contract;

namespace Domain
{
    public class RevenueUnit : IResponseData
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public String Name { get; set; } = default!;
        [JsonProperty("departmentId")]
        public int DepartmentId { get; set; }
    }
}

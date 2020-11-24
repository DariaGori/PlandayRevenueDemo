using System;
using Domain.Contract;
using Newtonsoft.Json;

namespace Domain
{
    public class EmployeeGroup : IResponseData
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public String Name { get; set; } = default!;
    }
}
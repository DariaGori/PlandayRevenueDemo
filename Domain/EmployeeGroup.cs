using System;
using Newtonsoft.Json;

namespace Domain
{
    public class EmployeeGroup
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public String Name { get; set; }
    }
}
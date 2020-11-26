using System;
using Newtonsoft.Json;

namespace Domain.RequestModel
{
    public class CreateDepartmentModel
    {
        [JsonProperty("name")] public String Name { get; set; } = default!;
        [JsonProperty("number")]
        public String? Number { get; set; }
    }
}

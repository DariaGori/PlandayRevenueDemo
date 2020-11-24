using Newtonsoft.Json;
using System;

namespace Domain
{
    public class CreateDepartmentRequestDto
    {
        [JsonProperty("name")] public String Name { get; set; } = default!;
        [JsonProperty("number")]
        public String? Number { get; set; }
    }
}

using Newtonsoft.Json;
using System;

namespace Domain
{
    public class CreateDepartmentRequestDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("number")]
        public string Number { get; set; }
    }
}

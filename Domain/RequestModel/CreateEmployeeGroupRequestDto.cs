using System;
using Newtonsoft.Json;

namespace Domain
{
    public class CreateEmployeeGroupRequestDto
    {
        [JsonProperty("name")]
        public String Name { get; set; }
    }
}
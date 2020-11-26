using System;
using Newtonsoft.Json;

namespace Domain.RequestModel
{
    public class CreateEmployeeGroupModel
    {
        [JsonProperty("name")]
        public String Name { get; set; } = default!;
    }
}
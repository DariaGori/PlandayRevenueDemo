using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class Department
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")] 
        public String Name { get; set; } = default!;
        [JsonProperty("number")]
        public String? Number { get; set; }
    }
}

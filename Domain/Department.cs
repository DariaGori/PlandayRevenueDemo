using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Domain.Contract;

namespace Domain
{
    public class Department : IResponseData
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")] 
        public String Name { get; set; } = default!;
        [JsonProperty("number")]
        public String? Number { get; set; }
    }
}

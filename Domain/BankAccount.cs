using Newtonsoft.Json;
using System;

namespace Domain
{
    public class BankAccount
    {
        [JsonProperty("registrationNumber")]
        public String? RegistrationNumber { get; set; }
        [JsonProperty("accountNumber")]
        public String? AccountNumber { get; set; }
    }
}

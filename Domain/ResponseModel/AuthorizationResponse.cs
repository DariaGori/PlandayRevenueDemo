using System;
using Newtonsoft.Json;

namespace Domain
{
    public class AuthorizationResponse
    {
        [JsonProperty("id_token")]
        public String IdToken { get; set; } = default!;
        [JsonProperty("access_token")]
        public String AccessToken { get; set; } = default!;
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("token_type")]
        public String TokenType { get; set; } = default!;
        [JsonProperty("refresh_token")]
        public String RefreshToken { get; set; } = default!;
        [JsonProperty("scope")]
        public String Scope { get; set; } = default!;
    }
}
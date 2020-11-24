using Newtonsoft.Json;

namespace Domain
{
    public class PostResponse<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; } = default!;
    }
}
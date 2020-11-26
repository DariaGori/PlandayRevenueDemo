using Newtonsoft.Json;

namespace Domain.ResponseModel
{
    public class PostModel<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; } = default!;
    }
}
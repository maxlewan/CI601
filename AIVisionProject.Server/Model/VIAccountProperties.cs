using System.Text.Json.Serialization;

namespace AIVisionProject.Server.Model
{
    public class VIAccountProperties
    {
        [JsonPropertyName("accountId")]
        public string Id { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace AIVisionProject.Server.Model
{
    public class VIVideo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("lastIndexed")]
        public DateTime LastIndexed { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }
    }
}

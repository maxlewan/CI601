using System.Text.Json.Serialization;

namespace AIVisionProject.Server.Model
{
    public class VIGetVideosResponse
    {
        [JsonPropertyName("results")]
        public List<VIVideo> Results { get; set; }
    }
}

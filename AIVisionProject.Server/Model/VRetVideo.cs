using System.Text.Json.Serialization;

namespace AIVisionProject.Server.Model
{
    public class VRetVideo
    {
        [JsonPropertyName("mode")]
        public string Mode { get; set; } = "add";

        [JsonPropertyName("documentId")]
        public string DocumentId { get; set; }

        [JsonPropertyName("documentUrl")]
        public string DocumentUrl { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace AIVisionProject.Server.Model
{
    public class VRetSearchResult
    {
        [JsonPropertyName("documentId")]
        public string DocumentId { get; set; }

        [JsonPropertyName("documentKind")]
        public string DocumentKind { get; set; }

        [JsonPropertyName("start")]
        public string Start { get; set; }

        [JsonPropertyName("end")]
        public string End { get; set; }

        [JsonPropertyName("best")]
        public string Best { get; set; }

        [JsonPropertyName("relevance")]
        public double Relevance { get; set; }
    }
}

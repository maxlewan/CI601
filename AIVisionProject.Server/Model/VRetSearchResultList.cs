using System.Text.Json.Serialization;

namespace AIVisionProject.Server.Model
{
    public class VRetSearchResultList
    {
        [JsonPropertyName("value")]
        public List<VRetSearchResult> Value { get; set; }
    }
}

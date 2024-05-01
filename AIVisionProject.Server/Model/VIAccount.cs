using System.Text.Json.Serialization;

namespace AIVisionProject.Server.Model
{
    public class VIAccount
    {
        [JsonPropertyName("properties")]
        public VIAccountProperties Properties { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }
    }
}

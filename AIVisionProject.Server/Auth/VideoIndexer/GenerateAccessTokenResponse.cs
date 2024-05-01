using System.Text.Json.Serialization;

namespace AIVisionProject.Server.Auth.VideoIndexer
{
    public class GenerateAccessTokenResponse
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
    }
}


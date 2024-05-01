using System.Text.Json.Serialization;

namespace AIVisionProject.Server.Auth.VideoIndexer
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ArmAccessTokenScope
    {
        Account,
        Project,
        Video
    }
}

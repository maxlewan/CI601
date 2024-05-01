using System.Data;

namespace AIVisionProject.Server.Model
{
    public class VideoWidgetsDto
    {
        public string VideoName { get; set; }
        public DateTime LastIndexed { get; set; }
        public string PlayerWidget { get; set; }
        public string InsightsWidget { get; set; }
    }
}

using AIVisionProject.Server.Model;
using AIVisionProject.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIVisionProject.Server.Controllers
{
    [ApiController]
    [Route("")]
    public class PreviouslyIndexedController : ControllerBase
    {
        private readonly IVideoAnalysisService _videoAnalysisService;

        public PreviouslyIndexedController(IStorageService storageService, IVideoAnalysisService videoAnalysisService)
        {
            _videoAnalysisService = videoAnalysisService;
        }


        [HttpGet("display-videos")]
        public async Task<IActionResult> GetPreviouslyIndexedVideos()
        {
            var widgetsList = await _videoAnalysisService.GetPreviouslyIndexedWidgets();

            return Ok(widgetsList);
        }

    }
}

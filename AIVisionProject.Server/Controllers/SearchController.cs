using AIVisionProject.Server.Model;
using AIVisionProject.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIVisionProject.Server.Controllers
{
    [ApiController]
    [Route("")]
    public class SearchController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly IVideoAnalysisService _videoAnalysisService;

        public SearchController(IStorageService storageService, IVideoAnalysisService videoAnalysisService)
        {
            _storageService = storageService;
            _videoAnalysisService = videoAnalysisService;
        }

        [HttpPost("search-display-videos")]
        public async Task<IActionResult> IndexAndGetVideos([FromBody] VideoSearchRequestDto searchRequest)
        {
            await _videoAnalysisService.DeleteRetrievalIndex();

            DateTime start = searchRequest.StartTime ?? DateTime.MinValue;
            DateTime end = searchRequest.EndTime ?? DateTime.MinValue;

            var videosList = await _storageService.GetVideosAsync(start, end);

            if (videosList.Count == 0)
            {
                return NotFound("No video clips have been found for the specified time range");
            }

            await _videoAnalysisService.CreateRetrievalIngestion(videosList);
            var widgetsList = await _videoAnalysisService.UploadAndGetAllVideosWidgets(videosList);

            return Ok(widgetsList);
        }

        [HttpPost("search-in-videos")]
        public async Task<IActionResult> RetrieveVideos([FromBody] SearchQueryDto searchQuery)
        {
            if (searchQuery != null && !string.IsNullOrWhiteSpace(searchQuery.SearchQuery))
            {
                var search = searchQuery.SearchQuery.Trim();

                var searchResults = await _videoAnalysisService.SearchInVideos(search);

                return Ok(searchResults);
            }

            return BadRequest("No search query found");
           
        }
    }
}

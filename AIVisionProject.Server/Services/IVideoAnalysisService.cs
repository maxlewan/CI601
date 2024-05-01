using AIVisionProject.Server.Model;
using Azure.Storage.Blobs.Models;
using System.Threading.Tasks;

namespace AIVisionProject.Server.Services
{
    public interface IVideoAnalysisService
    {
        Task<List<VideoWidgetsDto>> UploadAndGetAllVideosWidgets(List<BlobItem> videosList);
        Task<List<VideoWidgetsDto>> GetPreviouslyIndexedWidgets();
        Task CreateRetrievalIngestion(List<BlobItem> videosList);
        Task<VRetSearchResultList> SearchInVideos(string search);
        Task DeleteRetrievalIndex();
    }
}

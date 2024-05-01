using AIVisionProject.Server.Model;
using AIVisionProject.Server.Helpers;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs.Models;
using System.Runtime.CompilerServices;
using AIVisionProject.Server.Auth.VideoIndexer;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.VisualStudio.Services.Account;
using System.Data;
using Azure;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using System.Net.Http.Json;
using System.Text;
using System.Net.Http;

namespace AIVisionProject.Server.Services
{
    public class VideoIndexerService : IVideoAnalysisService
    {
        private readonly IStorageService _storageService;
        private string _armAccessToken;
        private string _accountAccessToken;
        private VIAccount _account;
        private const string ExcludedAI = "";

        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

        public VideoIndexerService(IStorageService storageService) 
        { 
            _storageService = storageService;            
        }

        public async Task<List<VideoWidgetsDto>> UploadAndGetAllVideosWidgets(List<BlobItem> videosList)
        {
            await AuthenticateAsync();
            _account = await GetAccountAsync(Constants.ViAccountName);

            var uploadedVideos = await UploadAllVideos(videosList);
            List<VideoWidgetsDto> widgetsList = new List<VideoWidgetsDto>();

            foreach (var video in uploadedVideos)
            {
                await WaitForIndexAsync(video.Id);
                VideoWidgetsDto vidWidgets = await GetVideoWidgets(video);
                widgetsList.Add(vidWidgets);
            }
            return widgetsList;
        }

        public async Task<List<VideoWidgetsDto>> GetPreviouslyIndexedWidgets()
        {
            await AuthenticateAsync();
            _account = await GetAccountAsync(Constants.ViAccountName);

            var indexedVideos = await GetIndexedVideos();
            List<VideoWidgetsDto> widgetsList = new List<VideoWidgetsDto>();

            foreach (var video in indexedVideos)
            {
                VideoWidgetsDto vidWidgets = await GetVideoWidgets(video);
                widgetsList.Add(vidWidgets);
            }
            return widgetsList;
        }

        public async Task CreateRetrievalIngestion(List<BlobItem> videosList)
        {
            await CreateRetrievalIndex();
            await CreateIngestion(videosList);
        }

        public async Task<VRetSearchResultList> SearchInVideos(string search)
        {
            var searchRequest = new VRetSearchRequest
            {
                QueryText = search,
                Filters = new VRetFilters
                {
                    FeatureFilters = new List<string> { "vision" }
                }
            };

            string jsonString = JsonSerializer.Serialize(searchRequest, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var url = $"https://projectmultiservice.cognitiveservices.azure.com/computervision/retrieval/indexes/{Constants.VideoRetrievalIndex}:queryByText?api-version=2023-05-01-preview";
            var client = HttpClientHelper.CreateHttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Constants.OcpApimSubKey);

            var searchRequestResult = await client.PostAsync(url, content);
            if (!searchRequestResult.IsSuccessStatusCode)
            {
                var errorContent = await searchRequestResult.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorContent}");
            }

            var searchResult = await searchRequestResult.Content.ReadAsStringAsync();

            var relevantTimeStamps = JsonSerializer.Deserialize<VRetSearchResultList>(searchResult);
            if (relevantTimeStamps == null)
            {
                Console.WriteLine("Deserialization failed or no data.");
                return null;
            }

            return relevantTimeStamps;
        }

        
        private async Task<List<VIVideo>> UploadAllVideos(List<BlobItem> videosList)
        {
            List<VIVideo> videoList = new List<VIVideo>();
            foreach (var video in videosList)
            {
                var uploadedVideo = await UploadVideo(video);
                videoList.Add(uploadedVideo);
            }
            return videoList;
        }

        private async Task<VideoWidgetsDto> GetVideoWidgets(VIVideo video)
        {
            string playerWidgetUrl = await GetPlayerWidgetUrlAsync(video.Id);
            string insightsWidgetUrl = await GetInsightsWidgetUrlAsync(video.Id);
            var vidWidgets = new VideoWidgetsDto
            {
                VideoName = video.Name,
                LastIndexed = video.LastIndexed,
                PlayerWidget = playerWidgetUrl,
                InsightsWidget = insightsWidgetUrl
            };
            return vidWidgets;
        }

        private async Task<List<VIVideo>> GetIndexedVideos()
        {
            if (_account == null)
            {
                throw new Exception("Call Get Account Details First");
            }

            var url = $"{Constants.ApiEndpoint}/{_account.Location}/Accounts/{_account.Properties.Id}/Videos";
            var client = HttpClientHelper.CreateHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountAccessToken);
            var getVideosRequestResult = await client.GetAsync(url);

            if (!getVideosRequestResult.IsSuccessStatusCode)
            {
                var errorContent = await getVideosRequestResult.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorContent}");
            }

            var getVideosResult = await getVideosRequestResult.Content.ReadAsStringAsync();

            var response = JsonSerializer.Deserialize<VIGetVideosResponse>(getVideosResult);

            List<VIVideo> videos = new List<VIVideo>();

            if (response != null && response.Results != null)
            {
                videos.AddRange(response.Results);
            }
            return videos;
        }


        private async Task<VIVideo> UploadVideo(BlobItem video)
        {
            string videoUrl = _storageService.GetVideoUri(video.Name);
            return await UploadUrlAsync(videoUrl, video.Name, ExcludedAI, false);
        }
        
        private async Task AuthenticateAsync()
        {
            try
            {
                _armAccessToken = await VIAccountTokenProvider.GetArmAccessTokenAsync();
                _accountAccessToken = await VIAccountTokenProvider.GetAccountAccessTokenAsync(_armAccessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private async Task<VIAccount> GetAccountAsync(string accountName)
        {
            if (_account != null)
            {
                return _account;
            }
            Console.WriteLine($"Getting account {accountName}.");
            try
            {
                // Set request uri
                var requestUri = $"{Constants.AzureResourceManager}/subscriptions/{Constants.SubscriptionId}/resourcegroups/{Constants.ResourceGroup}/providers/Microsoft.VideoIndexer/accounts/{accountName}?api-version={Constants.VIApiVersion}";
                var client = HttpClientHelper.CreateHttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _armAccessToken);

                var result = await client.GetAsync(requestUri);

                result.VerifyStatus(System.Net.HttpStatusCode.OK);
                var jsonResponseBody = await result.Content.ReadAsStringAsync();
                var account = JsonSerializer.Deserialize<VIAccount>(jsonResponseBody);
                VerifyValidAccount(account, accountName);
                Console.WriteLine($"[Account Details] Id:{account.Properties.Id}, Location: {account.Location}");
                _account = account;
                return account;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private async Task<VIVideo> UploadUrlAsync(string videoUrl, string videoName, string exludedAIs = null, bool waitForIndex = false)
        {
            if (_account == null)
            {
                throw new Exception("Call Get Account Details First");
            }

            Console.WriteLine($"Video for account {_account.Properties.Id} is starting to upload.");

            try
            {
                //Build Query Parameter Dictionary
                var queryDictionary = new Dictionary<string, string>
                {
                    { "name", videoName },
                    { "description", "video_description" },
                    { "privacy", "private" },
                    { "accessToken" , _accountAccessToken },
                    { "videoUrl" , videoUrl }
                };

                if (!Uri.IsWellFormedUriString(videoUrl, UriKind.Absolute))
                {
                    throw new ArgumentException("VideoUrl or LocalVideoPath are invalid");
                }

                var queryParams = queryDictionary.CreateQueryString();
                if (!string.IsNullOrEmpty(exludedAIs))
                    queryParams += AddExcludedAIs(exludedAIs);

                // Send POST request
                var url = $"{Constants.ApiEndpoint}/{_account.Location}/Accounts/{_account.Properties.Id}/Videos?{queryParams}";
                var client = HttpClientHelper.CreateHttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountAccessToken);
                var uploadRequestResult = await client.PostAsync(url, null);
                if (!uploadRequestResult.IsSuccessStatusCode)
                {
                    var errorContent = await uploadRequestResult.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {errorContent}");
                }

                var uploadResult = await uploadRequestResult.Content.ReadAsStringAsync();

                var uploadedVideo = JsonSerializer.Deserialize<VIVideo>(uploadResult);
                Console.WriteLine($"Video ID {uploadedVideo} was uploaded successfully");

                if (waitForIndex)
                {
                    Console.WriteLine("Waiting for Index Operation to Complete");
                    await WaitForIndexAsync(uploadedVideo.Id);
                }
                return uploadedVideo;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private async Task WaitForIndexAsync(string videoId)
        {
            Console.WriteLine($"Waiting for video {videoId} to finish indexing.");
            while (true)
            {
                var queryParams = new Dictionary<string, string>()
                {
                    {"language", "English"},
                }.CreateQueryString();

                var requestUrl = $"{Constants.ApiEndpoint}/{_account.Location}/Accounts/{_account.Properties.Id}/Videos/{videoId}/Index?{queryParams}";

                var client = HttpClientHelper.CreateHttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accountAccessToken);
                var videoGetIndexRequestResult = await client.GetAsync(requestUrl);
                videoGetIndexRequestResult.VerifyStatus(System.Net.HttpStatusCode.OK);
                var videoGetIndexResult = await videoGetIndexRequestResult.Content.ReadAsStringAsync();
                string processingState = JsonSerializer.Deserialize<VIVideo>(videoGetIndexResult).State;

                // If job is finished
                if (processingState == VIProcessingState.Processed.ToString())
                    if (processingState == VIProcessingState.Processed.ToString())
                    {
                        Console.WriteLine($"The video index has completed. Here is the full JSON of the index for video ID {videoId}: \n{videoGetIndexResult}");
                        return;
                    }
                    else if (processingState == VIProcessingState.Failed.ToString())
                    {
                        Console.WriteLine($"The video index failed for video ID {videoId}.");
                        throw new Exception(videoGetIndexResult);
                    }

                // Job hasn't finished
                Console.WriteLine($"The video index state is {processingState}");
                await Task.Delay(_pollingInterval);
            }
        }

        private async Task<string> GetPlayerWidgetUrlAsync(string videoId)
        {
            Console.WriteLine($"Getting the player widget URL for video {videoId}");

            try
            {
                var _vidAccessToken = await VIAccountTokenProvider.GetVideoAccessTokenAsync(_armAccessToken, videoId);
                var requestUrl = $"{Constants.ApiEndpoint}/{_account.Location}/Accounts/{_account.Properties.Id}/Videos/{videoId}/PlayerWidget";
                var client = HttpClientHelper.CreateHttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _vidAccessToken);
                var playerWidgetRequestResult = await client.GetAsync(requestUrl);

                playerWidgetRequestResult.VerifyStatus(System.Net.HttpStatusCode.MovedPermanently);
                var playerWidgetLink = playerWidgetRequestResult.Headers.Location;
                string playerWidgetLinkString = playerWidgetLink.ToString();
                Console.WriteLine($"Got the player widget URL: {playerWidgetLink}");
                return playerWidgetLinkString ?? throw new InvalidOperationException("Failed to obtain player widget URL.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private async Task<string> GetInsightsWidgetUrlAsync(string videoId)
        {
            Console.WriteLine($"Getting the insights widget URL for video {videoId}");
            var queryParams = new Dictionary<string, string>()
            {
                {"widgetType", "Keywords"},
                {"allowEdit", "true"},
            }.CreateQueryString();
            try
            {
                var _vidAccessToken = await VIAccountTokenProvider.GetVideoAccessTokenAsync(_armAccessToken, videoId);
                var requestUrl = $"{Constants.ApiEndpoint}/{_account.Location}/Accounts/{_account.Properties.Id}/Videos/{videoId}/InsightsWidget?{queryParams}";
                var client = HttpClientHelper.CreateHttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _vidAccessToken);
                var insightsWidgetRequestResult = await client.GetAsync(requestUrl);
                insightsWidgetRequestResult.VerifyStatus(System.Net.HttpStatusCode.MovedPermanently);
                var insightsWidgetLink = insightsWidgetRequestResult.Headers.Location;
                string insightsWidgetLinkString = insightsWidgetLink.ToString();
                Console.WriteLine($"Got the insights widget URL: {insightsWidgetLink}");
                return insightsWidgetLinkString ?? throw new InvalidOperationException("Failed to obtain insights widget URL.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }


        private static void VerifyValidAccount(VIAccount account, string accountName)
        {
            if (string.IsNullOrWhiteSpace(account.Location) || account.Properties == null || string.IsNullOrWhiteSpace(account.Properties.Id))
            {
                Console.WriteLine($"{nameof(accountName)} {accountName} not found. Check {nameof(Constants.SubscriptionId)}, {nameof(Constants.ResourceGroup)}, {nameof(accountName)} ar valid.");
                throw new Exception($"Account {accountName} not found.");
            }
        }

        private string AddExcludedAIs(string ExcludedAI)
        {
            if (string.IsNullOrEmpty(ExcludedAI))
            {
                return "";
            }
            var list = ExcludedAI.Split(',');
            return list.Aggregate("", (current, item) => current + ("&ExcludedAI=" + item));
        }

        private async Task CreateRetrievalIndex()
        {
            string jsonString = @"
            {
              'metadataSchema': {
                'fields': [
                  {
                    'name': 'cameraId',
                    'searchable': false,
                    'filterable': true,
                    'type': 'string'
                  },
                  {
                    'name': 'timestamp',
                    'searchable': false,
                    'filterable': true,
                    'type': 'datetime'
                  }
                ]
              },
              'features': [
                {
                  'name': 'vision',
                  'domain': 'surveillance'
                },
                {
                  'name': 'speech'
                }
              ]
            }";
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var url = $"https://projectmultiservice.cognitiveservices.azure.com/computervision/retrieval/indexes/{Constants.VideoRetrievalIndex}?api-version=2023-05-01-preview";
            var client = HttpClientHelper.CreateHttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Constants.OcpApimSubKey);
            var createIndexResult = await client.PutAsync(url, content);
            if (!createIndexResult.IsSuccessStatusCode)
            {
                var errorContent = await createIndexResult.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorContent}");
            }
        }

        public async Task DeleteRetrievalIndex()
        {
            var url = $"https://projectmultiservice.cognitiveservices.azure.com/computervision/retrieval/indexes/{Constants.VideoRetrievalIndex}?api-version=2023-05-01-preview";
            var client = HttpClientHelper.CreateHttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Constants.OcpApimSubKey);
            await client.DeleteAsync(url);
            Task.Delay(_pollingInterval).Wait();
        }

        private async Task CreateIngestion(List<BlobItem> videos)
        {
            var videosToSearch = new List<VRetVideo>();

            foreach (var video in videos)
            {

                var videoName = video.Name;
                var videoUrl = _storageService.GetVideoUri(videoName);
                VRetVideo videoToSearch = new VRetVideo
                {
                    DocumentId = videoName,
                    DocumentUrl = videoUrl,
                };
                videosToSearch.Add(videoToSearch);
            }
            var videoCollection = new VRetVideoGroup
            {
                Videos = videosToSearch
            };
            string jsonString = JsonSerializer.Serialize(videoCollection, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var url = $"https://projectmultiservice.cognitiveservices.azure.com/computervision/retrieval/indexes/{Constants.VideoRetrievalIndex}/ingestions/my-ingestion?api-version=2023-05-01-preview";
            var client = HttpClientHelper.CreateHttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Constants.OcpApimSubKey);
            await client.PutAsync(url, content);
        }

    }
}

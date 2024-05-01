using System.Web;

namespace AIVisionProject.Server.Helpers
{
    public static class HttpClientHelper
    {
        public static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
            };
            var httpClient = new HttpClient(handler);
            return httpClient;
        }

        public static string CreateQueryString(this IDictionary<string, string> parameters)
        {
            var queryParameters = HttpUtility.ParseQueryString(string.Empty);
            foreach (var parameter in parameters)
            {
                queryParameters[parameter.Key] = parameter.Value;
            }
            return queryParameters.ToString();
        }

        public static void VerifyStatus(this HttpResponseMessage response, System.Net.HttpStatusCode excpectedStatusCode)
        {
            if (response.StatusCode != excpectedStatusCode)
            {
                throw new Exception(response.ToString());
            }
        }
    }
}

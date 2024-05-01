using AIVisionProject.Server.Helpers;
using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AIVisionProject.Server.Auth.VideoIndexer
{
    public class VIAccountTokenProvider
    {
        public static async Task<string> GetArmAccessTokenAsync(CancellationToken ct = default)
        {
            var tokenRequestContext = new TokenRequestContext(new[] { $"{Constants.AzureResourceManager}/.default" });
            var tokenRequestResult = await new DefaultAzureCredential().GetTokenAsync(tokenRequestContext, ct);
            return tokenRequestResult.Token;
        }

        public static async Task<string> GetAccountAccessTokenAsync(string armAccessToken, ArmAccessTokenPermission permission = ArmAccessTokenPermission.Contributor, ArmAccessTokenScope scope = ArmAccessTokenScope.Account, CancellationToken ct = default)
        {
            var accessTokenRequest = new VIAccessTokenRequest
            {
                PermissionType = permission,
                Scope = scope
            };

            try
            {
                var jsonRequestBody = JsonSerializer.Serialize(accessTokenRequest);
                Console.WriteLine($"Getting Account access token: {jsonRequestBody}");
                var httpContent = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");

                // Set request uri
                var requestUri = $"{Constants.AzureResourceManager}/subscriptions/{Constants.SubscriptionId}/resourcegroups/{Constants.ResourceGroup}/providers/Microsoft.VideoIndexer/accounts/{Constants.ViAccountName}/generateAccessToken?api-version={Constants.VIApiVersion}";
                var client = HttpClientHelper.CreateHttpClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", armAccessToken);

                var result = await client.PostAsync(requestUri, httpContent, ct);
                result.EnsureSuccessStatusCode();
                var jsonResponseBody = await result.Content.ReadAsStringAsync(ct);
                Console.WriteLine($"Got Account access token: {scope} , {permission}");
                return JsonSerializer.Deserialize<GenerateAccessTokenResponse>(jsonResponseBody)?.AccessToken!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        public static async Task<string> GetVideoAccessTokenAsync(string armAccessToken, string videoId, ArmAccessTokenPermission permission = ArmAccessTokenPermission.Contributor, ArmAccessTokenScope scope = ArmAccessTokenScope.Video, CancellationToken ct = default)
        {
            var accessTokenRequest = new VIAccessTokenRequest
            {
                PermissionType = permission,
                Scope = scope,
                VideoId = videoId
            };

            try
            {
                var jsonRequestBody = JsonSerializer.Serialize(accessTokenRequest);
                Console.WriteLine($"Getting Account access token: {jsonRequestBody}");
                var httpContent = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");

                // Set request uri
                var requestUri = $"{Constants.AzureResourceManager}/subscriptions/{Constants.SubscriptionId}/resourcegroups/{Constants.ResourceGroup}/providers/Microsoft.VideoIndexer/accounts/{Constants.ViAccountName}/generateAccessToken?api-version={Constants.VIApiVersion}";
                var client = HttpClientHelper.CreateHttpClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", armAccessToken);

                
                var result = await client.PostAsync(requestUri, httpContent, ct);
                result.EnsureSuccessStatusCode();
                var jsonResponseBody = await result.Content.ReadAsStringAsync(ct);
                Console.WriteLine($"Got Account access token: {scope} , {permission}");
                return JsonSerializer.Deserialize<GenerateAccessTokenResponse>(jsonResponseBody)?.AccessToken!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}

namespace AIVisionProject.Server
{
    public class Constants
    {
        public const string VIApiVersion = "2024-01-01";
        public const string AzureResourceManager = "https://management.azure.com";
        public static readonly string VideoRetrievalIndex = Environment.GetEnvironmentVariable("VID_RETRIEVAL_INDEX") ?? "brandnewvidindex";
        public static readonly string SubscriptionId = Environment.GetEnvironmentVariable("AZ_SUBSCRIPTION_ID");
        public static readonly string ResourceGroup = Environment.GetEnvironmentVariable("PROJECT_RESOURCE_GROUP");
        public static readonly string ViAccountName = Environment.GetEnvironmentVariable("VI_ACCOUNT_NAME");
        public static readonly string ApiEndpoint = Environment.GetEnvironmentVariable("VI_API_ENDPOINT") ?? "https://api.videoindexer.ai";
        public static readonly Uri keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("KeyVaultUri"));
        public static readonly string StorageAccountName = Environment.GetEnvironmentVariable("AZ_STORAGE_ACCOUNT_NAME");
        public static readonly string OcpApimSubKey = Environment.GetEnvironmentVariable("OCP_APIM_SUB_KEY");
    }
}

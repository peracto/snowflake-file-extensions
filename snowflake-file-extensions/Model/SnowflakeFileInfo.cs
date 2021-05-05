using Newtonsoft.Json;

namespace Snowflake.FileStream.Model
{
    public class SnowflakeFileInfo
    {
        [JsonProperty(PropertyName = "locationType")]
        public string LocationType { get; set; }

        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }

        [JsonProperty(PropertyName = "path")] public string Path { get; set; }

        [JsonProperty(PropertyName = "region")]
        public string Region { get; set; }

        [JsonProperty(PropertyName = "storageAccount")]
        public string StorageAccount { get; set; }

        [JsonProperty(PropertyName = "isClientSideEncrypted")]
        public bool IsClientSideEncrypted { get; set; }

        [JsonProperty(PropertyName = "presignedUrl")]
        public string PresignedUrl { get; set; }

        [JsonProperty(PropertyName = "endPoint")]
        public string EndPoint { get; set; }

        [JsonProperty(PropertyName = "creds")] public SnowflakeCloudCredentials Credentials { get; set; }
    }
}
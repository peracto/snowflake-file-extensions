using Newtonsoft.Json;

namespace Snowflake.FileStream.Model
{
    public class SnowflakeCloudCredentials
    {
        [JsonProperty(PropertyName = "AWS_KEY_ID")]
        public string AwsKeyId { get; set; }

        [JsonProperty(PropertyName = "AWS_SECRET_KEY")]
        public string AwsSecretKey { get; set; }

        [JsonProperty(PropertyName = "AWS_TOKEN")]
        public string AwsToken { get; set; }

        [JsonProperty(PropertyName = "AWS_ID")]
        public string AwsId { get; set; }

        [JsonProperty(PropertyName = "AWS_KEY")]
        public string AwsKey { get; set; }
    }
}
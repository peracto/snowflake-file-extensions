using Newtonsoft.Json;

namespace Snowflake.FileStream.Model
{
    public class SnowflakeEncryptionMaterial
    {
        [JsonProperty(PropertyName = "queryStageMasterKey")]
        public string QueryStageMasterKey { get; set; }

        [JsonProperty(PropertyName = "queryId")]
        public string QueryId { get; set; }

        [JsonProperty(PropertyName = "smkId")] public long SmkId { get; set; }
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Snowflake.FileStream.Model
{
        public class SnowflakePutResponse
        {
            [JsonProperty(PropertyName = "src_locations", NullValueHandling = NullValueHandling.Ignore)]
            public List<string> SourceLocations { get; set; }

            [JsonProperty(PropertyName = "parallel")]
            public int Parallel { get; set; }

            [JsonProperty(PropertyName = "threshold")]
            public int Threshold { get; set; }

            [JsonProperty(PropertyName = "autoCompress")]
            public bool AutoCompress { get; set; }

            [JsonProperty(PropertyName = "overwrite")]
            public bool Overwrite { get; set; }

            [JsonProperty(PropertyName = "sourceCompression")]
            public string SourceCompression { get; set; }

            [JsonProperty(PropertyName = "clientShowEncryptionParameter")]
            public bool ClientShowEncryptionParameter { get; set; }

            [JsonProperty(PropertyName = "queryId")]
            public string QueryId { get; set; }

            [JsonProperty(PropertyName = "encryptionMaterial")]
            public SnowflakeEncryptionMaterial EncryptionMaterial { get; set; }

            [JsonProperty(PropertyName = "operation")]
            public string Operation { get; set; }

            [JsonProperty(PropertyName = "command")]
            public string Command { get; set; }

            [JsonProperty(PropertyName = "kind", NullValueHandling = NullValueHandling.Ignore)]
            public string Kind { get; set; }

            [JsonProperty(PropertyName = "uploadInfo", NullValueHandling = NullValueHandling.Ignore)]
            public SnowflakeFileInfo UploadInfo { get; set; }

            [JsonProperty(PropertyName = "stageInfo", NullValueHandling = NullValueHandling.Ignore)]
            public SnowflakeFileInfo StageInfo { get; set; }
        }
}
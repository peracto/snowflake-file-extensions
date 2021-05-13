using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Snowflake.FileStream.Model;

namespace Snowflake.FileStream
{


    internal class PutFileS3 : IPutFile
    {
        internal static IPutFile Create(SnowflakePutResponse Response)
        {
            var stageInfo = Response.StageInfo;
            var r = Amazon.RegionEndpoint.GetBySystemName(stageInfo.Region);
            var creds = stageInfo.Credentials;

            var meta = CryptoManager.CreateCrypto(
                Response.EncryptionMaterial.QueryStageMasterKey
            );

            return new PutFileS3(
                new AmazonS3Client(creds.AwsKeyId, creds.AwsSecretKey, creds.AwsToken, r),
                ConvertToString(Response.EncryptionMaterial, meta.KeySize),
                BucketMeta.Create(stageInfo.Location),
                meta
            );
        }

        private AmazonS3Client Client { get; }
        private string MatDesc { get;  }
        private BucketMeta Bucket { get; }


        public EncryptionMeta Crypto { get; }

        private static string ConvertToString(SnowflakeEncryptionMaterial em, int keySize)
            => $"{{\"queryId\":\"{em.QueryId}\",\"smkId\":\"{em.SmkId}\",\"keySize\":\"{keySize}\"}}";

        private PutFileS3(AmazonS3Client client, string matDesc, BucketMeta bucket, EncryptionMeta crypto)
        {
            Client = client;
            MatDesc = matDesc;
            Bucket = bucket;
            Crypto = crypto;
        }

        Task IPutFile.Put(
            string file, 
            string key, 
            string sha256,
            CancellationToken token
            )
        {
            return Client.PutObjectAsync(
                new PutObjectRequest
                {
                    BucketName = Bucket.BucketName,
                    Key = Bucket.CreateKey(key),
                    ContentType = "text/plain",
                    FilePath = file
                },
                new[] {
                    ("Content-Type", "application/octet-stream"),
                    ("sfc-digest", sha256),
                    ("x-amz-key", Crypto.Key),
                    ("x-amz-iv", Crypto.Iv),
                    ("x-amz-matdesc", MatDesc)
                },
                token
            );
        }

        public void Dispose()
        {
            Crypto.Dispose();
            Client.Dispose();
        }
    }
}
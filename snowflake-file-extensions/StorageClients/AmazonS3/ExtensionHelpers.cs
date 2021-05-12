using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Snowflake.FileStream.Model;

namespace Snowflake.FileStream
{
    internal static class ExtensionHelpers2
    {
        public static Task<PutObjectResponse> PutObjectAsync(this AmazonS3Client client, PutObjectRequest request,
            IEnumerable<(string, string)> kvps, CancellationToken token)
        {
            var metadata = request.Metadata;
            foreach (var (key, value) in kvps)
                metadata.Add(key, value);
            return client.PutObjectAsync(request, token);
        }

        public static PutObjectRequest AppendMetadata(this PutObjectRequest request, params (string, string)[] kvps)
        {
            var metadata = request.Metadata;
            foreach (var (key, value) in kvps)
                metadata.Add(key, value);
            return request;
        }



        public static async Task<bool> ObjectExistsAsync(this AmazonS3Client client, string bucketName, string key,
            CancellationToken token)
        {
            try
            {
                await client.GetObjectMetadataAsync(new GetObjectMetadataRequest()
                {
                    BucketName   = bucketName,
                    Key = key
                }, token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static System.Object _clientLock = new System.Object();
        private static AmazonS3Client _client = null;
        
        
        public static AmazonS3Client Get(this SnowflakeCloudCredentials creds, string region)  
        {
            return _client ?? GetX(creds, region);
        }
        public static AmazonS3Client GetX(this SnowflakeCloudCredentials creds, string region)
        {
            lock(_clientLock)
            {
                if (_client != null) return _client;
                System.Console.WriteLine("******************Client Created.");
                var r = Amazon.RegionEndpoint.GetBySystemName(region);
                return new AmazonS3Client(creds.AwsKeyId, creds.AwsSecretKey, creds.AwsToken, r);
            }
        }


    }
}
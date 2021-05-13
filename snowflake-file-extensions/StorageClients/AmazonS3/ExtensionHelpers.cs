using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

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
    }
}
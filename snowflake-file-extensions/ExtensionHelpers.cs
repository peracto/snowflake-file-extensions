using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Snowflake.FileStream.Model;

namespace Snowflake.FileStream
{
    internal static class ExtensionHelpers
    {
        public static async Task EncryptFile(this ICryptoTransform transform, string inputFile, string outputFile)
        {
            await using var fsIn = File.OpenRead(inputFile);
            await using var fsOut = File.OpenWrite(outputFile);
            try
            {
                await using var csEncrypt2 = new CryptoStream(fsOut, transform, CryptoStreamMode.Write);
                await fsIn.CopyToAsync(csEncrypt2);
                if (!csEncrypt2.HasFlushedFinalBlock)
                    csEncrypt2.FlushFinalBlock();
            }
            finally
            {
                fsIn.Close();
                fsOut.Close();
            }
        }
        
        public static Task<PutObjectResponse> PutObjectAsync(this AmazonS3Client client, PutObjectRequest request,
            IEnumerable<(string, string)> kvps, CancellationToken token)
        {
            var metadata = request.Metadata;
            foreach (var (key, value) in kvps)
                metadata.Add(key, value);
            return client.PutObjectAsync(request, token);
        }

        public static async Task<bool> ObjectExistsAsync(this AmazonS3Client client, BucketMeta bucketMeta,
            CancellationToken token)
        {
            try
            {
                await client.GetObjectMetadataAsync(new GetObjectMetadataRequest()
                {
                    BucketName   = bucketMeta.BucketName,
                    Key = bucketMeta.Key
                }, token);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        
        public static AmazonS3Client Get(this SnowflakeCloudCredentials creds, string region)  
        {
            var r = Amazon.RegionEndpoint.GetBySystemName(region);
            return new AmazonS3Client(creds.AwsKeyId, creds.AwsSecretKey, creds.AwsToken, r);
        }
        
    }
}
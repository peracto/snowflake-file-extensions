using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Snowflake.FileStream.Model;

namespace Snowflake.FileStream
{
    public class PutResult
    {
        public readonly string Filename;
        public readonly string Result;

        public PutResult(string filename, string result)
        {
            Filename = filename;
            Result = result;
        }
    }
    public static class PutFiles
    {
        public static async Task<PutResult[]> Put(SnowflakePutResponse response, CancellationToken token)
        {
            var results = new List<PutResult>();
            var stageInfo = response.StageInfo;

            using var s3Client = stageInfo.Credentials.Get(
                stageInfo.Region
            );

            using var crypto = CryptoManager.CreateCrypto(
                response.EncryptionMaterial.QueryStageMasterKey
            );

            foreach (var file in response.SourceLocations)
            {
                var bucket = BucketMeta.Create(stageInfo.Location, file);

                if (!response.Overwrite && await s3Client.ObjectExistsAsync(bucket, token))
                {
                    results.Add(new PutResult(file, "SKIPPING"));
                    continue;
                }
                
                var compressFile = response.AutoCompress && !FileHelpers.IsCompressed(file)
                    ? Path.GetTempFileName()
                    : null;
                
                var encryptFile = Path.GetTempFileName();

                try
                {
                    if (compressFile != null)
                        FileHelpers.CompressFile(file, compressFile);
                    
                    await crypto.Transform.EncryptFile(
                        compressFile ?? file, 
                        encryptFile
                    );                    

                    var putResponse = await s3Client.PutObjectAsync(
                        new PutObjectRequest
                        {
                            BucketName = bucket.BucketName,
                            Key = bucket.Key,
                            ContentType = "text/plain",
                            FilePath = encryptFile,
                        },
                        new[]
                        {
                            ("Content-Type", "application/octet-stream"),
                            ("sfc-digest", FileHelpers.GetSha256Digest(compressFile ?? file)),
                            ("x-amz-key", crypto.Key),
                            ("x-amz-iv", crypto.Iv),
                            ("x-amz-matdesc", ConvertToString(response.EncryptionMaterial, crypto.KeySize))
                        },
                        token
                    );

                    results.Add(new PutResult(file,"OK"));
                }
                catch
                {
                    results.Add(new PutResult(file,"FAILED"));
                }
                finally
                {
                    if (compressFile != null) File.Delete(compressFile);
                    File.Delete(encryptFile);
                }
            }
            return results.ToArray();
        }

        private static string ConvertToString(SnowflakeEncryptionMaterial em, int keySize)
            => $"{{\"queryId\":\"{em.QueryId}\",\"smkId\":\"{em.SmkId}\",\"keySize\":\"{keySize}\"}}";
    }
}
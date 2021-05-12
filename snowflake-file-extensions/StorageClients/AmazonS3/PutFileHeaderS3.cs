using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Snowflake.FileStream.Model;

namespace Snowflake.FileStream
{

    public class PutFileHeaderS3 : IDisposable
    {
        private static void DummyEvent(PutFileEvent evt)
        {
        }

        internal static IEnumerable<IFileTask> Create(
            SnowflakePutResponse response,
            Action<PutFileEvent> eventCallback,
            IEnumerable<IPutFileItem> files
            )
        {
            PutFileHeaderS3 headerS3 = null;
            foreach (var file in files.OrderBy(e=>e.Key))
            {
                file.EncryptionMeta = CryptoManager.CreateCrypto(
                    response.EncryptionMaterial.QueryStageMasterKey
                );
                headerS3 ??= new PutFileHeaderS3(response, eventCallback ?? DummyEvent);
                yield return headerS3.CreateTask(file);
            }
        }

        private readonly Object _syncLock = new Object();
        private int _activeCount;
        private Action<PutFileEvent> PutFileEventCallback { get;  }
        private AmazonS3Client Client { get; set; }
        private BucketMeta Bucket { get; set; }
        private string MatDesc { get; set; }
        public SnowflakePutResponse Response {get;}

        private static string ConvertToString(SnowflakeEncryptionMaterial em, int keySize)
            => $"{{\"queryId\":\"{em.QueryId}\",\"smkId\":\"{em.SmkId}\",\"keySize\":\"{keySize}\"}}";

        private PutFileHeaderS3(SnowflakePutResponse response,Action<PutFileEvent> putFileEventCallback)
        {
            Response = response;
            PutFileEventCallback = putFileEventCallback;
        }

        private void EnsureClient(int keysize)
        {
            if (Client != null)
                return;

            lock (_syncLock)
            {
                if (Client != null)
                {
                    Console.WriteLine("************************************************SYNC LOCK");
                    return;
                }

                var stageInfo = Response.StageInfo;
                Client = stageInfo.Credentials.Get(stageInfo.Region);
                MatDesc = ConvertToString(Response.EncryptionMaterial, keysize);
                Bucket = BucketMeta.Create(stageInfo.Location);
            }
        }

        private async Task<PutResult> ExecutePut(IPutFileItem fileItem, CancellationToken token)
        {
            try
            {
                var crypto = fileItem.EncryptionMeta;
                EnsureClient(crypto.KeySize);
                var file = fileItem.Filename;
                var bucketKey = Bucket.CreateKey(fileItem.Key);

                if (!File.Exists(file))
                    return new PutResult(fileItem, "MISSING");

                if (!Response.Overwrite && await Client.ObjectExistsAsync(Bucket.BucketName, bucketKey, token))
                    return new PutResult(fileItem, "SKIPPING");

                var compressFile = Response.SourceCompression != "none" && Response.AutoCompress &&
                                   !FileHelpers.IsCompressed(file)
                    ? Path.GetTempFileName()
                    : null;

                var encryptFile = Path.GetTempFileName();

                try
                {
                    if (compressFile != null)
                        FileHelpers.CompressFile(file, compressFile, Response.SourceCompression);

                    Console.WriteLine($"Crypto:>:file {Thread.CurrentThread.ManagedThreadId}::{file} ");
                    await crypto.Transform.EncryptFile(
                        compressFile ?? file,
                        encryptFile
                    );
                    Console.WriteLine($"Crypto:<:file {Thread.CurrentThread.ManagedThreadId}::{file} ");
                    Console.WriteLine($"Putting:1:file {Thread.CurrentThread.ManagedThreadId}::{file} ");

                    var putResponse = await Client.PutObjectAsync(
                        new PutObjectRequest
                        {
                            BucketName = Bucket.BucketName,
                            Key = bucketKey,
                            ContentType = "text/plain",
                            FilePath = encryptFile
                        },
                        new[] {
                            ("Content-Type", "application/octet-stream"),
                            ("sfc-digest", FileHelpers.GetSha256Digest(compressFile ?? file)),
                            ("x-amz-key", crypto.Key),
                            ("x-amz-iv", crypto.Iv),
                            ("x-amz-matdesc", MatDesc)
                        },
                        token
                    );

                    Console.WriteLine($"Putting:2:file {Thread.CurrentThread.ManagedThreadId}::{file} ");
                    if (putResponse == null)
                        Console.WriteLine($"Putting:2: null");
                    else Console.WriteLine($"Putting:2: exp:{putResponse.Expiration}:etag:{putResponse.ETag}:status:{putResponse.HttpStatusCode}:charged:{putResponse.RequestCharged}::{putResponse.VersionId}");

                    return new PutResult(fileItem, "OK");
                }
                catch (Exception ex)
                {
                    return new PutResult(fileItem, "FAILED", ex);
                }
                finally
                {
                    crypto?.Dispose();
                    if (compressFile != null) File.Delete(compressFile);
                    File.Delete(encryptFile);
                }
            }
            finally
            {
                //  if (Interlocked.Decrement(ref _activeCount) == 0)
                //  {
                //      _putFileEventCallback(new PutFileEvent());
                //      Dispose();
                //  }
            }
        }

        public void Dispose()
        {
            Client?.Dispose();
        }

        private IFileTask CreateTask(IPutFileItem filename)
        {
            _activeCount++;
            return new PutFileTask(this, filename);
        }

        private class PutFileTask : IFileTask
        {
            private readonly PutFileHeaderS3 _headerS3;
            private readonly IPutFileItem _fileName;

            public PutFileTask(PutFileHeaderS3 headerS3, IPutFileItem fileName)
            {
                _headerS3 = headerS3;
                _fileName = fileName;
            }

            public Task<PutResult> Execute(CancellationToken token)
                => _headerS3.ExecutePut(_fileName, token);
        }
    }
}
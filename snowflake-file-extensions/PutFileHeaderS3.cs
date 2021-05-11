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
    public class PutFileEvent
    {
    }

    public class PutFileItem
    {
        public readonly string Filename;
        public readonly string Key;

        public PutFileItem(string filename, string key)
        {
            Filename = filename;
            Key = key;
        }
        public PutFileItem(string filename)
        {
            Filename = filename;
            Key = Path.GetFileName(filename);
        }
    }
    
    public class PutFileHeaderS3 : IDisposable
    {
        private static void DummyEvent(PutFileEvent evt)
        {
        }
        
        public static IEnumerable<IFileTask> Create(SnowflakePutResponse response, Action<PutFileEvent> eventCallback, IEnumerable<PutFileItem> files)
        {
            PutFileHeaderS3 headerS3 = null;
            foreach (var file in files)
            {
                headerS3 ??= new PutFileHeaderS3(response, eventCallback ?? DummyEvent);
                yield return headerS3.CreateTask(file);
            }
        }

        private readonly SnowflakePutResponse _response;
        private AmazonS3Client s3Client;
        private EncryptionMeta crypto;
        private BucketMeta bucket;
        private string _matDesc;
        private int _activeCount;
        private readonly Action<PutFileEvent> _putFileEventCallback;

        private static string ConvertToString(SnowflakeEncryptionMaterial em, int keySize)
            => $"{{\"queryId\":\"{em.QueryId}\",\"smkId\":\"{em.SmkId}\",\"keySize\":\"{keySize}\"}}";

        private PutFileHeaderS3(SnowflakePutResponse response, Action<PutFileEvent> putFileEventCallback)
        {
            _response = response;
            _putFileEventCallback = putFileEventCallback;
        }

        private void EnsureClient()
        {
            if (s3Client != null)
                return;

            lock (_response)
            {
                var stageInfo = _response.StageInfo;

                s3Client = stageInfo.Credentials.Get(
                    stageInfo.Region
                );

                crypto = CryptoManager.CreateCrypto(
                    _response.EncryptionMaterial.QueryStageMasterKey
                );

                _matDesc = ConvertToString(_response.EncryptionMaterial, crypto.KeySize);

                bucket = BucketMeta.Create(stageInfo.Location);
            }
        }

        private async Task<PutResult> ExecutePut(PutFileItem fileItem, CancellationToken token)
        {
            try
            {
                EnsureClient();
                var file = fileItem.Filename;
                var bucketKey = bucket.CreateKey(fileItem.Key);

                if (!File.Exists(file))
                    return new PutResult(file, "MISSING");

                if (!_response.Overwrite && await s3Client.ObjectExistsAsync(bucket.BucketName, bucketKey, token))
                    return new PutResult(file, "SKIPPING");

                var compressFile = _response.SourceCompression != "none" && _response.AutoCompress &&
                                   !FileHelpers.IsCompressed(file)
                    ? Path.GetTempFileName()
                    : null;

                var encryptFile = Path.GetTempFileName();

                try
                {
                    if (compressFile != null)
                        FileHelpers.CompressFile(file, compressFile, _response.SourceCompression);

                    await crypto.Transform.EncryptFile(
                        compressFile ?? file,
                        encryptFile
                    );

                    var putResponse = await s3Client.PutObjectAsync(
                        new PutObjectRequest
                        {
                            BucketName = bucket.BucketName,
                            Key = bucketKey,
                            ContentType = "text/plain",
                            FilePath = encryptFile,
                        },
                        new[]
                        {
                            ("Content-Type", "application/octet-stream"),
                            ("sfc-digest", FileHelpers.GetSha256Digest(compressFile ?? file)),
                            ("x-amz-key", crypto.Key),
                            ("x-amz-iv", crypto.Iv),
                            ("x-amz-matdesc", _matDesc)
                        },
                        token
                    );
                    
                    return new PutResult(file, "OK");
                }
                catch (Exception ex)
                {
                    return new PutResult(file, "FAILED", ex);
                }
                finally
                {
                    if (compressFile != null) File.Delete(compressFile);
                    File.Delete(encryptFile);
                }
            }
            finally
            {
                if (Interlocked.Decrement(ref _activeCount) == 0)
                {
                    _putFileEventCallback(new PutFileEvent());
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            crypto?.Dispose();
            s3Client?.Dispose();
        }

        private IFileTask CreateTask(PutFileItem filename)
        {
            _activeCount++;
            return new PutFileTask(this, filename);
        }

        private class PutFileTask : IFileTask
        {
            private readonly PutFileHeaderS3 _headerS3;
            private readonly PutFileItem _fileName;

            public PutFileTask(PutFileHeaderS3 headerS3, PutFileItem fileName)
            {
                _headerS3 = headerS3;
                _fileName = fileName;
            }

            public Task<PutResult> Execute(CancellationToken token)
                => _headerS3.ExecutePut(_fileName, token);
        }
    }
}
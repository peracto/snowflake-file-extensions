using System.IO;

namespace Snowflake.FileStream
{
    internal class BucketMeta
    {
        public readonly string BucketName;
        public readonly string BucketPath;

        public static BucketMeta Create(string location)
        {
            var p = location.IndexOf('/');
            return new BucketMeta(
                p == -1 ? location : location[..p],
                p == -1 ? "" : location[(p + 1)..]
            );
        }

        public string CreateKey(string key)
            => $"{BucketPath}/{key}";

        private BucketMeta(string bucketName, string path)
        {
            BucketName = bucketName;
            BucketPath = path;
        }
    }
}
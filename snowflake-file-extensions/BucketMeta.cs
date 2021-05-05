using System.IO;

namespace Snowflake.FileStream
{
    internal class BucketMeta
    {
        public readonly string BucketName;
        public readonly string Key;

        public static BucketMeta Create(string location, string filename)
        {
            var p = location.IndexOf('/');
            var path = p == -1 ? "" : location[(p + 1)..];
            return new BucketMeta(
                p == -1 ? location : location[..p],
                $"{path}/{Path.GetFileName(filename)}"
            );
        }

        private BucketMeta(string bucketName, string key)
        {
            BucketName = bucketName;
            Key = key;
        }
    }
}
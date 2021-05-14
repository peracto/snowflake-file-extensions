namespace Snowflake.FileStream.StorageClients.AmazonS3
{
    internal class BucketMeta
    {
        public readonly string BucketName;
        private readonly string _bucketPath;

        public static BucketMeta Create(string location)
        {
            var p = location.IndexOf('/');
            return new BucketMeta(
                p == -1 ? location : location[..p],
                p == -1 ? "" : location[(p + 1)..]
            );
        }

        public string CreateKey(string key)
            => $"{_bucketPath}/{key}";

        private BucketMeta(string bucketName, string path)
        {
            BucketName = bucketName;
            _bucketPath = path;
        }
    }
}
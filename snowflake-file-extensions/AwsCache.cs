using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Amazon.S3;

namespace Snowflake.FileStream
{
    internal static class AwsCache
    {
        private static IDictionary<string, AmazonS3Client> _cache = new ConcurrentDictionary<string, AmazonS3Client>();

        public static AmazonS3Client Get(string key, string secret, string token, string region)
        {
            var k = $"{key}*{secret}*{token}*{region}";
            if (_cache.TryGetValue(k, out var client))
                return client;
            Console.WriteLine("Creating new S3 Client", k);
            var r = Amazon.RegionEndpoint.GetBySystemName(region);
            client = new AmazonS3Client(key, secret, token, r);
            _cache.TryAdd(k, client);
            return client;
        }
    }
}
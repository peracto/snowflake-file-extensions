using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace Snowflake.FileStream
{
    internal static class FileHelpers
    {
        public static bool IsCompressed(string file)
            => file.EndsWith(".gz");

        public static void CompressFile(string file, string outputPath)
        {
            using var originalFileStream = File.OpenRead(file);
            using var compressedFileStream = File.OpenWrite(outputPath);
            using var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress);
            originalFileStream.CopyTo(compressionStream);
        }

        public static string GetSha256Digest(string path)
        {
            using var stream = File.OpenRead(path);
            using var m = SHA256.Create();
            var hash = m.ComputeHash(stream);
            return Convert.ToBase64String(hash);
        }
    }
}
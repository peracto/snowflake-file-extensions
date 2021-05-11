using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace Snowflake.FileStream
{
    internal static class FileHelpers
    {
        public static bool IsCompressed(string file)
            =>  file.EndsWith(".gz") || file.EndsWith(".brotli") || file.EndsWith(".br");

        public static void CompressFileGZip(string file, string outputPath)
        {
            using var originalFileStream = File.OpenRead(file);
            using var compressedFileStream = File.OpenWrite(outputPath);
            using var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress);
            originalFileStream.CopyTo(compressionStream);
        }

        public static void CompressFile(string file, string outputPath, string compressionType)
        {
            switch (compressionType)
            {

                case "brotli":
                    CompressFileBrotli(file, outputPath);
                    return;
                default:
                    CompressFileGZip(file, outputPath);
                    return;
            }
        }
        
        public static void CompressFileBrotli(string file, string outputPath)
        {
            using var originalFileStream = File.OpenRead(file);
            using var compressedFileStream = File.OpenWrite(outputPath);
            using var compressionStream = new BrotliStream(compressedFileStream, CompressionMode.Compress);
            originalFileStream.CopyTo(compressionStream);
        }
        

        public static string GetSha256Digest(string path)
        {
            using var stream = File.OpenRead(path);
            using var m = SHA256.Create();
            var hash = m.ComputeHash(stream);
            return Convert.ToBase64String(hash);
        }
        
        public static string NormalisePath(string path)
        {
            var p = path.Trim();
            return Path.GetFullPath(
                p.StartsWith('~')
                    ? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), p.Substring(1))
                    : p
            );
        }
    }
    
}
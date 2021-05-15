using System;
using System.Threading;
using System.Threading.Tasks;

namespace Snowflake.FileStream
{
    public interface IPutFile : IDisposable
    {
        Task Put(
            string file,
            string key,
            string sha256,
            CancellationToken token
            );

        bool IsExpired { get; }

        bool IsCompressed { get; }

        string SourceCompression { get; }

        Task<string> EncryptFile(string inputFile, string outputFile);
    }
}
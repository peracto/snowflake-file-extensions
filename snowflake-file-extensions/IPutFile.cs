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

        EncryptionMeta Crypto { get; }
    }
}
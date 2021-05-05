using System;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Snowflake.FileStream
{
    internal class EncryptionMeta : IDisposable
    {
        public string Key;
        public string Iv;
        public int KeySize;
        public ICryptoTransform Transform;
        
        public void Dispose()
        {
            if (Transform == null) return;
            Transform.Dispose();
            Transform = null;
        }
    }
}
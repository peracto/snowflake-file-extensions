using System;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Snowflake.FileStream
{
    public class EncryptionMeta : IDisposable
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

        public async Task EncryptFile(string inputFile, string outputFile)
        {
            using (var fsIn = File.OpenRead(inputFile))
            using (var fsOut = File.OpenWrite(outputFile))
            using (var csEncrypt2 = new CryptoStream(fsOut, Transform, CryptoStreamMode.Write))
            {
                await fsIn.CopyToAsync(csEncrypt2);
                if (!csEncrypt2.HasFlushedFinalBlock)
                    csEncrypt2.FlushFinalBlock();
                csEncrypt2.Close();
                fsOut.Close();
                fsIn.Close();
            }
        }
    }
}
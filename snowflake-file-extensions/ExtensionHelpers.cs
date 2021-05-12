using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Snowflake.FileStream.Model;

namespace Snowflake.FileStream
{
    internal static class ExtensionHelpers
    {
        public static async Task EncryptFile(this ICryptoTransform transform, string inputFile, string outputFile)
        {
            using (var fsIn = File.OpenRead(inputFile))
            using (var fsOut = File.OpenWrite(outputFile))
            using (var csEncrypt2 = new CryptoStream(fsOut, transform, CryptoStreamMode.Write))
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
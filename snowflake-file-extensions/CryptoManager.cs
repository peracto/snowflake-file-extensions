using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Snowflake.FileStream
{
    internal static class CryptoManager
    {
        public static EncryptionMeta CreateCrypto(
            string queryStageMasterKey
        )
        {
            var decodedKey = Convert.FromBase64String(queryStageMasterKey);
            var fileKey = CreateSecureRandom(decodedKey.Length);
            var ivKey = CreateSecureRandom(decodedKey.Length);
            var result = EncryptKey(decodedKey, fileKey);

            return new EncryptionMeta
            {
                Key = Convert.ToBase64String(result),
                Iv = Convert.ToBase64String(ivKey),
                KeySize = decodedKey.Length * 8,
                Transform = CreateEncryptor(fileKey, ivKey)
            };
        }

        private static readonly RNGCryptoServiceProvider Provider = new RNGCryptoServiceProvider();
        
        private static byte[] CreateSecureRandom(int length)
        {
            var array = new byte[length];
            Provider.GetBytes(array);
            return array;
        }
        
        private static readonly List<Aes> Cryptos = new List<Aes>();
        private static Aes lastCbc = null;
        private static Aes lastEcb = null;

        private static Aes GetCryptoCbc(int keySize)
        {
            if (lastCbc == null || lastCbc.KeySize != keySize)
                lastCbc = GetOrCreate(keySize, CipherMode.CBC);
            return lastCbc;
        }

        private static ICryptoTransform CreateEncryptor(byte[] fileKey, byte[] ivKey)
        {
            return GetCryptoCbc(fileKey.Length * 8).CreateEncryptor(fileKey, ivKey);
        }

        private static Aes GetCryptoEcb(int keySize)
        {
            if (lastEcb == null || lastEcb.KeySize != keySize)
                lastEcb = GetOrCreate(keySize, CipherMode.ECB);
            return lastEcb;
        }

        private static byte[] EncryptKey(byte[] decodedKey, byte[] fileKey)
        {
            using var cipher = GetCryptoEcb(fileKey.Length * 8).CreateEncryptor(decodedKey, null!);
            return cipher.TransformFinalBlock(fileKey, 0, fileKey.Length);
        }

        private static Aes GetOrCreate(int keySize, CipherMode mode)
        {
            foreach (var c in Cryptos)
                if (c.KeySize == keySize && c.Mode == mode)
                    return c;

            var x = new AesCryptoServiceProvider
            {
                KeySize = keySize,
                Padding = PaddingMode.PKCS7,
                Mode = mode
            };
            Cryptos.Add(x);
            return x;
        }
    }
}
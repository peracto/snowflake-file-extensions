using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Snowflake.FileStream
{
    internal static class CryptoManager
    {
        private static readonly RNGCryptoServiceProvider Provider = new RNGCryptoServiceProvider();
        private static InternalCryptoManager _internalCryptoManager = new InternalCryptoManager(-1, null, null);

        public static CryptoMeta CreateCrypto(string masterKey)
        {
            var decodedKey = Convert.FromBase64String(masterKey);
            var fileKey = CreateSecureRandom(decodedKey.Length);
            var ivKey = CreateSecureRandom(decodedKey.Length);

            if (_internalCryptoManager.KeySize != fileKey.Length)
                _internalCryptoManager = C.GetOrCreate(fileKey.Length);

            return new CryptoMeta
            {
                Key = Convert.ToBase64String(_internalCryptoManager.EncryptKey(decodedKey, fileKey)),
                Iv = Convert.ToBase64String(ivKey),
                KeySize = decodedKey.Length * 8,
                Transform = _internalCryptoManager.CreateEncryptor(fileKey, ivKey)
            };
        }

        private class InternalCryptoManager
        {
            public int KeySize { get; }
            private Aes Cbc { get; }
            private Aes Ecb { get; }

            public InternalCryptoManager(int keySize, Aes cbc, Aes ecb)
            {
                KeySize = keySize;
                Cbc = cbc;
                Ecb = ecb;
            }

            public byte[] EncryptKey(byte[] decodedKey, byte[] fileKey)
            {
                using var cipher = Ecb.CreateEncryptor(decodedKey, null!);
                return cipher.TransformFinalBlock(fileKey, 0, fileKey.Length);
            }

            public ICryptoTransform CreateEncryptor(byte[] fileKey, byte[] ivKey) 
                => Cbc.CreateEncryptor(fileKey, ivKey);
        }

        private static class C
        {
            private static readonly List<InternalCryptoManager> Cryptos = new List<InternalCryptoManager>();

            public static InternalCryptoManager GetOrCreate(int keySize)
            {
                lock (Cryptos)
                {
                    foreach (var c in Cryptos)
                        if (c.KeySize == keySize)
                            return c;
                    return Add(Create(keySize));
                }
            }

            private static InternalCryptoManager Create(int keySize) =>
                new InternalCryptoManager(
                    keySize,
                    new AesCryptoServiceProvider
                    {
                        KeySize = keySize * 8,
                        Padding = PaddingMode.PKCS7,
                        Mode = CipherMode.CBC
                    },
                    new AesCryptoServiceProvider
                    {
                        KeySize = keySize * 8,
                        Padding = PaddingMode.PKCS7,
                        Mode = CipherMode.ECB
                    }
                );

            private static InternalCryptoManager Add(InternalCryptoManager internalCryptoManager)
            {
                Cryptos.Add(internalCryptoManager);
                return internalCryptoManager;
            }
        }

        private static byte[] CreateSecureRandom(int length)
        {
            var array = new byte[length];
            Provider.GetBytes(array);
            return array;
        }
    }
}
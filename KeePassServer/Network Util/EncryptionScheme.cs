﻿using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;

//==========================================================================================
//
//        filename : EncryptionScheme.cs
//        description : This is an encryption utility class. 
//                      It is used to implemented salted AES encryption & DH key exchange
//        created by Erni Gao at  Nov 2020
//   
//==========================================================================================

namespace KeePassServer.Network_Util
{
    class EncryptionScheme
    {
        // parameters used for AES encryption & decryption
        private const int BLOCKSIZE = 128;  //block size = the length of IV
        private const int KEYSIZE = 256;
        private const PaddingMode PADDING = PaddingMode.PKCS7;
        private const CipherMode MODE = CipherMode.CBC;

        /// <summary>
        /// use AES to encrypt data with salted initial vector 
        /// </summary>
        /// <param name="msg">plain text message in byte array</param>
        /// <param name="key">key used for AES encryption</param>
        /// <returns>encrypted messages in byte array</returns>
        public static byte[] saltedEncryption(byte[] msg, byte[] key)
        {
            byte[] saltByte = SHA512.Create().ComputeHash(key);
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.KeySize = KEYSIZE;
            aes.BlockSize = BLOCKSIZE;
            aes.Padding = PADDING;
            aes.Mode = MODE;
            Rfc2898DeriveBytes derivedKey = new Rfc2898DeriveBytes(key, saltByte, 1000);
            aes.Key = derivedKey.GetBytes(aes.KeySize / 8);
            aes.IV = derivedKey.GetBytes(aes.BlockSize / 8);

            ICryptoTransform crypto = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] encryptedMsg = crypto.TransformFinalBlock(msg, 0, msg.Length);
            crypto.Dispose();
            return encryptedMsg;

        }

        /// <summary>
        /// use AES to decrypt data with salted intial vector
        /// </summary>
        /// <param name="encryptedMsg">message encrypted under AES</param>
        /// <param name="key">key used for AES decryption</param>
        /// <returns>plain text message in byte array</returns>
        public static byte[] saltedDecryption(byte[] encryptedMsg, byte[] key)
        {
            byte[] saltByte = SHA512.Create().ComputeHash(key);
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = BLOCKSIZE;
            aes.KeySize = KEYSIZE;
            aes.Padding = PADDING;
            aes.Mode = MODE;

            Rfc2898DeriveBytes derivedKey = new Rfc2898DeriveBytes(key, saltByte, 1000);
            aes.Key = derivedKey.GetBytes(aes.KeySize / 8);
            aes.IV = derivedKey.GetBytes(aes.BlockSize / 8);
            ICryptoTransform crypto = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] decrypted = crypto.TransformFinalBlock(encryptedMsg, 0, encryptedMsg.Length);
            crypto.Dispose();
            return decrypted;
        }

        /// <summary>
        /// use diffie hellman to generate one-time key
        /// </summary>
        /// <param name="ecd">>ECDiffieHellmanCng object to generate public key by Elliptic Curve Diffie-Hellman algorithm</param>
        /// <param name="publicKey">public key generated by ECDiffieHellmanCng object</param>
        public static void publicKeyGenerator(out ECDiffieHellmanCng ecd, out byte[] publicKey)
        {
            ecd = new ECDiffieHellmanCng();
            ecd.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            ecd.HashAlgorithm = CngAlgorithm.Sha256;
            publicKey = ecd.PublicKey.ToByteArray();
        }

        /// <summary>
        /// derive a common key together with another party's public key
        /// </summary>
        /// <param name="ecd">ECDiffieHellmanCng object used to derive common key</param>
        /// <param name="anotherKey">another party's public key</param>
        /// <returns>a common key that can be used for AES encryption & decryption</returns>
        public static byte[] deriveCommonKey(ECDiffieHellmanCng ecd, byte[] anotherKey)
        {
            byte[] key = ecd.DeriveKeyMaterial(CngKey.Import(anotherKey, CngKeyBlobFormat.EccPublicBlob));
            ecd.Dispose();
            return key;
        }
    }
}

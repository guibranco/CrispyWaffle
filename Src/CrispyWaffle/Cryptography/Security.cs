using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CrispyWaffle.Cryptography
{
    /// <summary>
    /// Class Security.
    /// </summary>
    public static class Security
    {
        /// <summary>
        /// Encrypts the specified plain text.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <param name="passwordHash">The password hash</param>
        /// <param name="saltKey">The salt key</param>
        /// <param name="viKey">The vi key</param>
        /// <returns>String.</returns>
        public static string Encrypt(
            this string plainText,
            string passwordHash,
            string saltKey,
            string viKey
        )
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            var keyBytes = new Rfc2898DeriveBytes(
                passwordHash,
                Encoding.ASCII.GetBytes(saltKey)
            ).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.Zeros
            };
            var encryption = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(viKey));

            byte[] cipherTextBytes;

            var memoryStream = new MemoryStream();

            using (
                var cryptoStream = new CryptoStream(
                    memoryStream,
                    encryption,
                    CryptoStreamMode.Write
                )
            )
            {
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                cipherTextBytes = memoryStream.ToArray();
            }

            return Convert.ToBase64String(cipherTextBytes);
        }

        /// <summary>
        /// Decrypts the specified encrypted text.
        /// </summary>
        /// <param name="encryptedText">The encrypted text.</param>
        /// <param name="passwordHash">The password hash</param>
        /// <param name="saltKey">The salt key</param>
        /// <param name="viKey">The vi key</param>
        /// <returns>String.</returns>
        [Localizable(false)]
        public static string Decrypt(
            this string encryptedText,
            string passwordHash,
            string saltKey,
            string viKey
        )
        {
            var cipherTextBytes = Convert.FromBase64String(encryptedText);
            var keyBytes = new Rfc2898DeriveBytes(
                passwordHash,
                Encoding.ASCII.GetBytes(saltKey)
            ).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.None
            };

            var decryption = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(viKey));
            var memoryStream = new MemoryStream(cipherTextBytes);

            byte[] plainTextBytes;
            int decryptedByteCount;

            using (
                var cryptoStream = new CryptoStream(memoryStream, decryption, CryptoStreamMode.Read)
            )
            {
                plainTextBytes = new byte[cipherTextBytes.Length];
                decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            }

            return Encoding
                .UTF8.GetString(plainTextBytes, 0, decryptedByteCount)
                .TrimEnd("\0".ToCharArray());
        }

        /// <summary>
        /// Generates a hash for the requested value using the desired hash algorithm
        /// </summary>
        /// <param name="value">The value to compute the hash</param>
        /// <param name="type">The hash algorithm</param>
        /// <returns>The hash of the value</returns>
        public static string Hash(string value, HashAlgorithmType type)
        {
            if (!_hashAlgorithms.ContainsKey(type))
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid algorithm type");
            }

            var algorithm = _hashAlgorithms[type];

            var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));

            var result = new StringBuilder();

            foreach (var t in hash)
            {
                result.Append(
                    type == HashAlgorithmType.Md5
                        ? t.ToString(@"x2")
                        : t.ToString(CultureInfo.InvariantCulture)
                );
            }

            return result.ToString();
        }

        /// <summary>
        /// The hash algorithms
        /// </summary>
        private static Dictionary<HashAlgorithmType, HashAlgorithm> _hashAlgorithms =
            new Dictionary<HashAlgorithmType, HashAlgorithm>
            {
                { HashAlgorithmType.Md5, new MD5CryptoServiceProvider() },
                { HashAlgorithmType.Sha1, SHA1.Create() },
                { HashAlgorithmType.Sha256, SHA256.Create() },
                { HashAlgorithmType.Sha384, SHA384.Create() },
                { HashAlgorithmType.Sha512, SHA512.Create() }
            };
    }
}

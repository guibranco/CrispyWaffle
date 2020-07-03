namespace CrispyWaffle.Cryptography
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

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
        public static string Encrypt(this string plainText, string passwordHash, string saltKey, string viKey)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            var keyBytes = new Rfc2898DeriveBytes(passwordHash, Encoding.ASCII.GetBytes(saltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var encryption = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(viKey));

            var memoryStream = new MemoryStream();

            using var cryptoStream = new CryptoStream(memoryStream, encryption, CryptoStreamMode.Write);

            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            var cipherTextBytes = memoryStream.ToArray();

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
        public static string Decrypt(this string encryptedText, string passwordHash, string saltKey, string viKey)
        {
            var cipherTextBytes = Convert.FromBase64String(encryptedText);
            var keyBytes = new Rfc2898DeriveBytes(passwordHash, Encoding.ASCII.GetBytes(saltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.None };

            var decryption = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(viKey));
            var memoryStream = new MemoryStream(cipherTextBytes);

            using var cryptoStream = new CryptoStream(memoryStream, decryption, CryptoStreamMode.Read);

            var plainTextBytes = new byte[cipherTextBytes.Length];
            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }

        /// <summary>
        /// Generates a hash for the requested value using the desired hash algorithm
        /// </summary>
        /// <param name="value">The value to compute the hash</param>
        /// <param name="type">The hash algorithm</param>
        /// <returns>The hash of the value</returns>
        public static string Hash(string value, HashAlgorithmType type)
        {
            HashAlgorithm algorithm;
            switch (type)
            {
                case HashAlgorithmType.MD5:
                    algorithm = new MD5CryptoServiceProvider();
                    break;
                case HashAlgorithmType.SHA1:
                    algorithm = SHA1.Create();
                    break;
                case HashAlgorithmType.SHA256:
                    algorithm = SHA256.Create();
                    break;
                case HashAlgorithmType.SHA384:
                    algorithm = SHA384.Create();
                    break;
                case HashAlgorithmType.SHA512:
                    algorithm = SHA512.Create();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
            var result = new StringBuilder();
            foreach (var t in hash)
                result.Append(type == HashAlgorithmType.MD5
                                  ? t.ToString(@"x2")
                                  : t.ToString(CultureInfo.InvariantCulture));
            return result.ToString();
        }
    }
}

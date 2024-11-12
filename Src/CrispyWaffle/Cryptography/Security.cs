using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CrispyWaffle.Cryptography;

/// <summary>
/// Class Security.
/// </summary>
public static class Security
{
    /// <summary>
    /// Encrypts the provided plain text using a password hash, salt key, and initialization vector key.
    /// </summary>
    /// <param name="plainText">The plain text string to be encrypted.</param>
    /// <param name="passwordHash">The password hash used for deriving the encryption key.</param>
    /// <param name="saltKey">The salt key used in the key derivation process.</param>
    /// <param name="viKey">The initialization vector key used for the encryption process.</param>
    /// <returns>A base64-encoded string representing the encrypted cipher text.</returns>
    /// <remarks>
    /// This method utilizes the Rijndael symmetric encryption algorithm in CBC mode with zero padding.
    /// It first derives a key from the provided password hash and salt key using the Rfc2898DeriveBytes class.
    /// The plain text is then encrypted using the derived key and the provided initialization vector key.
    /// The resulting cipher text is converted to a base64 string for easy storage and transmission.
    /// Note that the use of zero padding may lead to security vulnerabilities; consider using a more secure padding mode.
    /// </remarks>
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
            Padding = PaddingMode.Zeros,
        };
        var encryption = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(viKey));

        byte[] cipherTextBytes;

        var memoryStream = new MemoryStream();

        using (
            var cryptoStream = new CryptoStream(memoryStream, encryption, CryptoStreamMode.Write)
        )
        {
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            cipherTextBytes = memoryStream.ToArray();
        }

        return Convert.ToBase64String(cipherTextBytes);
    }

    /// <summary>
    /// Decrypts an encrypted string using the specified password hash, salt key, and initialization vector key.
    /// </summary>
    /// <param name="encryptedText">The base64-encoded string that needs to be decrypted.</param>
    /// <param name="passwordHash">The password hash used for generating the key for decryption.</param>
    /// <param name="saltKey">The salt key used in the key derivation function.</param>
    /// <param name="viKey">The initialization vector key used for the decryption process.</param>
    /// <returns>The decrypted plain text as a string.</returns>
    /// <remarks>
    /// This method utilizes the Rijndael symmetric encryption algorithm in CBC mode to decrypt the provided
    /// base64-encoded string. It first derives a cryptographic key from the provided password hash and salt
    /// using the Rfc2898DeriveBytes class. The decryption process is performed using a CryptoStream, which
    /// reads from a memory stream containing the cipher text bytes. The resulting plain text is then converted
    /// from bytes to a string, with any trailing null characters removed. This method assumes that the input
    /// string is properly formatted and that the decryption parameters are correct.
    /// </remarks>
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
            Padding = PaddingMode.None,
        };

        var decryption = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(viKey));
        var memoryStream = new MemoryStream(cipherTextBytes);

        byte[] plainTextBytes;
        int decryptedByteCount;

        using (var cryptoStream = new CryptoStream(memoryStream, decryption, CryptoStreamMode.Read))
        {
            plainTextBytes = new byte[cipherTextBytes.Length];
            decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
        }

        return Encoding
            .UTF8.GetString(plainTextBytes, 0, decryptedByteCount)
            .TrimEnd("\0".ToCharArray());
    }

    /// <summary>
    /// Generates a hash for the requested value using the desired hash algorithm.
    /// </summary>
    /// <param name="value">The value to compute the hash.</param>
    /// <param name="type">The hash algorithm.</param>
    /// <returns>The hash of the value.</returns>
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
    /// The hash algorithms.
    /// </summary>
    private static readonly Dictionary<HashAlgorithmType, HashAlgorithm> _hashAlgorithms =
        new()
        {
            { HashAlgorithmType.Md5, MD5.Create() },
            { HashAlgorithmType.Sha1, SHA1.Create() },
            { HashAlgorithmType.Sha256, SHA256.Create() },
            { HashAlgorithmType.Sha384, SHA384.Create() },
            { HashAlgorithmType.Sha512, SHA512.Create() },
        };
}

namespace CrispyWaffle.Cryptography;

/// <summary>
/// Enum HashAlgorithmType.
/// </summary>
public enum HashAlgorithmType
{
    /// <summary>
    /// The MD5 algorithm.
    /// </summary>
    Md5,

    /// <summary>
    /// The SHA1 algorithm.
    /// </summary>
    Sha1,

    /// <summary>
    /// The SHA-256 algorithm.
    /// </summary>
    Sha256,

    /// <summary>
    /// The SHA-384 algorithm.
    /// </summary>
    Sha384,

    /// <summary>
    /// The SHA-512 algorithm.
    /// </summary>
    Sha512,

#if NET8_0_OR_GREATER
    /// <summary>
    /// The SHA3-256 algorithm. Available in .NET 8.0 and later.
    /// </summary>
    Sha3_256,

    /// <summary>
    /// The SHA3-384 algorithm. Available in .NET 8.0 and later.
    /// </summary>
    Sha3_384,

    /// <summary>
    /// The SHA3-512 algorithm. Available in .NET 8.0 and later.
    /// </summary>
    Sha3_512,
#endif
}

# Cryptography

The cryptography module has one static class, `Security`. It provides symmetric string encryption and decryption (AES-256 in CBC mode, with a key derived from a password) and string hashing with MD5, SHA-1, SHA-2 and, on .NET 8 or later, SHA-3.

These helpers exist for compatibility with data produced by earlier versions of the library. Read [Security notes](#security-notes) before you use them for new data.

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.Cryptography` |

## How it works

### Encryption and decryption

`Encrypt` and `Decrypt` are extension methods on `string`. Both take the same three secrets: `passwordHash`, `saltKey` and `viKey`.

| Step | Detail |
|---|---|
| Key derivation | `new Rfc2898DeriveBytes(passwordHash, Encoding.ASCII.GetBytes(saltKey)).GetBytes(32)`. This is PBKDF2 with the .NET defaults for this constructor: HMAC-SHA1 and 1,000 iterations. The password is UTF-8 encoded by `Rfc2898DeriveBytes`; the salt is ASCII encoded (non-ASCII characters become `?`). The result is a 256-bit key. |
| Initialization vector | `Encoding.ASCII.GetBytes(viKey)`, used as is. It must be exactly 16 ASCII characters (one 128-bit block). Any other length throws `ArgumentException` ("Specified initialization vector (IV) does not match the block size for this algorithm."). |
| Cipher | `RijndaelManaged` with its default 128-bit block size, `CipherMode.CBC`. With a 256-bit key this is AES-256-CBC. |
| Padding | `Encrypt` uses `PaddingMode.Zeros`. `Decrypt` uses `PaddingMode.None` and then removes all trailing `'\0'` characters from the decoded text. |
| Plain text encoding | UTF-8. |
| Cipher text encoding | Base64 of the raw cipher text. The output contains only the cipher text: no salt, IV, version or authentication tag. |

Encryption is deterministic: the same plain text with the same three secrets always gives the same output.

### Hashing

`Security.Hash(value, type)` UTF-8 encodes `value`, computes the hash with the selected algorithm and converts the bytes to a string. The string format depends on the algorithm:

| Algorithm | Output format |
|---|---|
| `Md5` | Lowercase hexadecimal, two characters per byte (32 characters). This is the usual MD5 hex string. |
| All others | Each byte as an unpadded decimal number (`0` to `255`), concatenated with no separator. For example, bytes `0x05 0xA0` become `"5160"`. |

The non-MD5 format is not the standard hex encoding, so it does not match the output of other tools (`sha256sum`, `Convert.ToHexString`), and it has variable length. Different hashes can also produce the same string, because the byte boundaries are lost. Compare these values only with other values produced by `Security.Hash`.

The hash algorithm objects are created once, in a static field, and shared by all calls.

## Getting started

```csharp
using System;
using CrispyWaffle.Cryptography;

const string password = "123456";
const string salt = "12345678";
const string iv = "HZEM7|Ne2YGS/F41"; // exactly 16 ASCII characters

var encrypted = "PlainText".Encrypt(password, salt, iv); // "vBWW26e4f9QMRPjz4pXoEQ=="
var decrypted = encrypted.Decrypt(password, salt, iv);   // "PlainText"

var md5 = Security.Hash("PlainText", HashAlgorithmType.Md5); // "b7ebbf7f254ef646928dd58f62383a85"

Console.WriteLine($"{encrypted} {decrypted} {md5}");
```

## Reference

### `Security`

`public static class Security` — string encryption, decryption and hashing helpers.

| Member | Description |
|---|---|
| `Encrypt(this string plainText, string passwordHash, string saltKey, string viKey)` | Encrypts `plainText` with AES-256-CBC and zero padding, as described in [How it works](#encryption-and-decryption). Returns the cipher text as Base64. Throws `ArgumentException` when `viKey` is not 16 ASCII bytes. Throws `ArgumentNullException` when any argument is `null`. |
| `Decrypt(this string encryptedText, string passwordHash, string saltKey, string viKey)` | Decodes Base64 and decrypts with the same parameters. Returns the UTF-8 text with trailing `'\0'` characters removed. Throws `FormatException` for invalid Base64, `ArgumentException` for an invalid IV length, and `CryptographicException` when the cipher text length is not a multiple of 16 bytes. Wrong secrets do not throw; they return unreadable text. |
| `Hash(string value, HashAlgorithmType type)` | Returns the hash of `value` in the format described in [Hashing](#hashing). Throws `ArgumentOutOfRangeException` (`Invalid algorithm type`) for a `type` that is not supported on the current target framework. Throws `ArgumentNullException` when `value` is `null`. |

### `HashAlgorithmType`

`public enum HashAlgorithmType` — algorithm selector for `Security.Hash`.

| Value | Description |
|---|---|
| `Md5` | MD5 (128-bit). Output is lowercase hex. |
| `Sha1` | SHA-1 (160-bit). |
| `Sha256` | SHA-256. |
| `Sha384` | SHA-384. |
| `Sha512` | SHA-512. |
| `Sha3_256` | SHA3-256. Only defined when the library is compiled for .NET 8 or later. |
| `Sha3_384` | SHA3-384. Only defined when the library is compiled for .NET 8 or later. |
| `Sha3_512` | SHA3-512. Only defined when the library is compiled for .NET 8 or later. |

## Examples

### Hash with each algorithm

```csharp
using System;
using CrispyWaffle.Cryptography;

foreach (var type in new[] { HashAlgorithmType.Md5, HashAlgorithmType.Sha1, HashAlgorithmType.Sha256 })
{
    Console.WriteLine($"{type}: {Security.Hash("PlainText", type)}");
}

// Md5: b7ebbf7f254ef646928dd58f62383a85
// Sha1: 5022439192242211661341747135888248197192199115101
// Sha256: 43167247186156224140100585014234839723255150225212687216120565623113068401650
```

### Keep secrets out of code

Read the three secrets from configuration instead of constants. The IV still has to be 16 ASCII characters.

```csharp
using System;
using CrispyWaffle.Cryptography;

var password = Environment.GetEnvironmentVariable("APP_CRYPTO_PASSWORD");
var salt = Environment.GetEnvironmentVariable("APP_CRYPTO_SALT");
var iv = Environment.GetEnvironmentVariable("APP_CRYPTO_IV");

var token = "customer-42".Encrypt(password, salt, iv);
var original = token.Decrypt(password, salt, iv);
```

## Security notes

These points come from the code in `Security.cs`:

- **No integrity protection.** The output is unauthenticated AES-CBC. An attacker who can change the cipher text can change the decrypted plain text in predictable ways, and `Decrypt` will not detect it. Do not use this for data that crosses a trust boundary without a separate MAC or signature.
- **Fixed IV.** The IV comes from a caller-supplied string and is reused for every message. Equal plain texts produce equal cipher texts, and messages that share a prefix produce cipher texts that share a prefix. Each message should have a random IV.
- **Fixed salt.** The salt is also a caller-supplied constant, so the derived key is the same for every message.
- **Weak key derivation.** PBKDF2 uses the `Rfc2898DeriveBytes` constructor defaults: HMAC-SHA1 with 1,000 iterations, far below current recommendations. Treat `passwordHash` as a high-entropy secret, not a human password.
- **Zero padding.** Plain text that ends with `'\0'` characters loses them on decryption.
- **Obsolete APIs.** `RijndaelManaged` and the `Rfc2898DeriveBytes(string, byte[])` constructor are marked obsolete in recent .NET versions (SYSLIB0022 and SYSLIB0041).
- **Weak hash algorithms.** `Md5` and `Sha1` are broken for collision resistance. Do not use them for signatures or integrity checks. None of the hash options is suitable for password storage: they are fast, unsalted hashes. Use a password hashing function (PBKDF2 with a high iteration count, bcrypt, scrypt or Argon2) for passwords.
- **Thread safety.** `Hash` reuses one `HashAlgorithm` instance per algorithm, stored in a static dictionary. `HashAlgorithm.ComputeHash` is not thread-safe, so concurrent calls with the same algorithm can return wrong results or throw. Synchronize calls if you hash from several threads.
- **SHA-3 availability.** On .NET 8 or later, the SHA-3 instances are created in the same static initializer as the others. .NET throws `PlatformNotSupportedException` from `SHA3_256.Create()` and its siblings on platforms without SHA-3 support, which would surface as a `TypeInitializationException` the first time `Hash` runs on such a platform.

## See also

- [Extensions](extensions.md)
- [Utilities](utilities.md)

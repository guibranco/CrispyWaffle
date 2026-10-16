# Validations

The `CrispyWaffle.Validations` namespace validates and normalizes a small set of personal data formats: Brazilian taxpayer documents (CPF for individuals, CNPJ for companies) and e-mail addresses. It also exposes the precompiled regular expressions the rest of the library uses for IP addresses, e-mail addresses and string cleanup.

Use it when you accept CPF, CNPJ or e-mail input and want a check-digit validation without a third-party library. For formatting and other pt-BR specific conversions (such as formatting a CPF or parsing a Brazilian phone number), see the [pt-BR internationalization package](../packages/i18n-ptbr.md).

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.Validations` |

## How it works

### CPF and CNPJ

The document methods are string extension methods on `PersonalDataValidation`. They all share the same steps:

1. Remove every character that is not `0`–`9`, so formatted input such as `529.982.247-25` or `19.664.626/0001-20` is accepted.
2. Check the length: 11 digits for a CPF, 14 for a CNPJ. Otherwise throw `InvalidDocumentException`.
3. Reject input that contains 11 identical consecutive digits (`11111111111`, `00000000000000`, and so on) by throwing `InvalidDocumentException`.
4. Compute the two check digits with the standard modulus-11 algorithm. For each digit, the weighted sum is divided by 11; a remainder below 2 gives `0`, otherwise the digit is `11 - remainder`. A CPF uses weights 10 down to 2 (then 11 down to 2); a CNPJ uses `5,4,3,2,9,8,7,6,5,4,3,2` (then `6,5,4,3,2,9,8,7,6,5,4,3,2`).

`IsValidBrazilianPersonDocument` and `IsValidBrazilianCorporateDocument` then compare the computed digits with the end of the original string. So the `IsValid...` methods return `false` for a document with wrong check digits, but **throw** `InvalidDocumentException` for wrong length or repeated digits. Wrap them in a `try`/`catch` if you need a plain boolean.

Only numeric documents are supported. Letters are stripped in step 1, so an alphanumeric CNPJ fails the length check.

### E-mail addresses

`ValidateEmailAddress` matches the input against `NetworkValidations.EmailAddressPattern`, an RFC 5322-style expression that also accepts Unicode letters. It also repairs two common typos:

- An address that ends with `combr` (case-insensitive) has `combr` replaced with `com.br`.
- An address that contains a comma has each comma replaced with a dot.

The method returns the original or repaired address, or throws `InvalidEmailAddressException`. The repaired address is not matched against the pattern again.

## Getting started

```csharp
using System;
using CrispyWaffle.Validations;

try
{
    var cpfOk = "229.706.190-08".IsValidBrazilianPersonDocument();   // true
    var cnpjOk = "19664626000120".IsValidBrazilianCorporateDocument(); // true
    var email = "nobody@example.combr".ValidateEmailAddress();       // "nobody@example.com.br"
}
catch (InvalidDocumentException ex)
{
    Console.WriteLine(ex.Message);
}
catch (InvalidEmailAddressException ex)
{
    Console.WriteLine(ex.Message);
}
```

## Reference

### `PersonalDataValidation`

`public static class PersonalDataValidation` — validation and check-digit calculation for CPF and CNPJ, and e-mail validation.

| Member | Description |
|---|---|
| `ValidateEmailAddress(this string emailAddress)` | Returns the address unchanged when it matches `NetworkValidations.EmailAddressPattern` and does not end with `combr`. If it ends with `combr` or contains a comma, returns it with `combr` replaced by `com.br` (case-sensitive replace) and commas replaced by dots, without re-validating. Throws `InvalidEmailAddressException` when the input is null or whitespace (the message shows an empty address) or otherwise invalid. |
| `IsValidBrazilianPersonDocument(this string document)` | Returns `true` when `document` ends with the CPF check digits from `CalculateBrazilianPersonDocumentDigits`, otherwise `false`. Throws `InvalidDocumentException` (type `CPF`) when the input does not have 11 digits or contains 11 identical consecutive digits. |
| `IsValidBrazilianCorporateDocument(this string document)` | Returns `true` when `document` ends with the CNPJ check digits from `CalculateBrazilianCorporateDocument`, otherwise `false`. Throws `InvalidDocumentException` (type `CNPJ`) when the input does not have 14 digits or contains 11 identical consecutive digits. |
| `CalculateBrazilianPersonDocumentDigits(this string document)` | Returns the two CPF check digits as a string, computed from the first 9 digits. The input must still have 11 digits; the last two are ignored. Throws `InvalidDocumentException` under the same conditions as above. |
| `CalculateBrazilianCorporateDocument(this string document)` | Returns the two CNPJ check digits as a string, computed from the first 12 digits. The input must still have 14 digits. Throws `InvalidDocumentException` under the same conditions as above. |
| `SameNumberDocumentPattern` | `public static readonly Regex` matching 11 identical consecutive digits, `(\d)\1{10}`. It is not anchored, so it matches anywhere in a 14-digit CNPJ as well. |

All document methods throw `NullReferenceException` for null input.

### `NetworkValidations`

`public static class NetworkValidations` — precompiled regular expressions for network data. Both use a 5-second match timeout.

| Member | Description |
|---|---|
| `EmailAddressPattern` | `public static readonly Regex`. Anchored (`^...$`), case-insensitive pattern for e-mail addresses in the style of RFC 5322, including quoted local parts and Unicode letters in the local part and domain. The domain needs at least one dot. |
| `IpAddressPattern` | `public static readonly Regex` with pattern `\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}`. It is not anchored and does not check that each octet is 0–255, so `IsMatch` is `true` for `999.1.1.1` or for any text that contains an IPv4-like substring. Use `Match(...).Value` to extract an address, and `System.Net.IPAddress.TryParse` for strict validation. |

### `StringValidations`

`public static class StringValidations` — precompiled regular expressions used by the string helpers in [Extension methods](extensions.md). All use `CultureInvariant | IgnoreCase` and a 5-second timeout.

| Member | Pattern | Matches |
|---|---|---|
| `PortuguesePrepositionPattern` | `^(da\|de\|do\|das\|dos\|no\|na\|nos\|nas\|-\|etapa)$` | A whole string that is a Portuguese preposition (`da`, `de`, `do`, `das`, `dos`, `no`, `na`, `nos`, `nas`), `-` or `etapa`. Useful for keeping these words lowercase in title-cased names. |
| `ParenthesesPattern` | `\((.+?)\)` | Text inside parentheses; group 1 holds the content. |
| `NonAlphanumericPattern` | `[^\w\.@-]` | Any character that is not a word character, `.`, `@` or `-`. Used by `RemoveNonAlphanumeric` and `ReplaceNonAlphanumeric`. |
| `NonNumericPattern` | `[^0-9]` | Any character that is not an ASCII digit. Used by `RemoveNonNumeric`. |
| `SpacesPattern` | `\s+` | Runs of whitespace. Used by `RemoveSpaces`. |
| `MultipleSpacesPattern` | `[\t\|\s]{2,}` | Runs of two or more whitespace characters. Because `\|` inside a character class is a literal, runs of `\|` also match. Used by `RemoveExcessSpaces`. |
| `InvalidFileName` | built from `Path.GetInvalidFileNameChars()` | Runs of characters that are invalid in a file name, and trailing dots. The character set depends on the operating system. Used by `ToValidFileName`. |

### `InvalidDocumentException`

`public class InvalidDocumentException : Exception` — thrown by the CPF and CNPJ methods of `PersonalDataValidation` when the document has the wrong number of digits or is made of repeated digits. Marked `[Serializable]`.

| Member | Description |
|---|---|
| `InvalidDocumentException(string documentType, string value)` | Message: `The value '{value}' isn't a valid value for a document of type {documentType}`. The library passes `CPF` or `CNPJ` as the type and the digits-only value (punctuation removed) as the value. |
| `InvalidDocumentException(SerializationInfo info, StreamingContext context)` | `protected` deserialization constructor. |

### `InvalidEmailAddressException`

`public class InvalidEmailAddressException : Exception` — thrown by `ValidateEmailAddress` when the address is empty or cannot be matched or repaired. Marked `[Serializable]`.

| Member | Description |
|---|---|
| `InvalidEmailAddressException(string emailAddress)` | Message: `The e-mail address '{emailAddress}' isn't in a valid e-mail address format`. |
| `InvalidEmailAddressException(SerializationInfo info, StreamingContext context)` | `protected` deserialization constructor. |

## Examples

### Validate a CPF as a boolean

The `IsValid...` methods throw for malformed input. This wrapper turns every failure into `false`.

```csharp
using CrispyWaffle.Validations;

static bool IsCpf(string input)
{
    if (string.IsNullOrWhiteSpace(input))
    {
        return false;
    }

    try
    {
        return input.IsValidBrazilianPersonDocument();
    }
    catch (InvalidDocumentException)
    {
        return false;
    }
}

var a = IsCpf("135.117.490-85"); // true
var b = IsCpf("135.117.490-86"); // false: wrong check digit
var c = IsCpf("111.111.111-11"); // false: repeated digits
```

### Compute check digits

```csharp
using CrispyWaffle.Validations;

// The input must have 11 digits, but only the first 9 are used.
// Pad with two placeholder digits to complete a new CPF.
var digits = "22970619000".CalculateBrazilianPersonDocumentDigits(); // "08"
var cpf = "229706190" + digits;                                      // "22970619008"
```

### Extract an IP address from a header

```csharp
using CrispyWaffle.Validations;

var header = "for=203.0.113.7;proto=https";
var ip = NetworkValidations.IpAddressPattern.IsMatch(header)
    ? NetworkValidations.IpAddressPattern.Match(header).Value   // "203.0.113.7"
    : null;
```

## See also

- [pt-BR internationalization](../packages/i18n-ptbr.md) for formatting documents and other Brazilian conversions
- [Extension methods](extensions.md) for the string helpers that use these patterns
- [Utilities](utilities.md)

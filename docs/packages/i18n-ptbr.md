# CrispyWaffle.I18n.PtBr

CrispyWaffle.I18n.PtBr adds string extension methods for Brazilian data: parsing Brazilian phone numbers and formatting CPF/CNPJ documents and CEP zip codes. Use it when you receive these values as free text (with or without punctuation) and need a normalized value or a display format.

| | |
|---|---|
| **Package** | `CrispyWaffle.I18n.PtBr` |
| **Namespace** | `CrispyWaffle.I18n.PtBr` |
| **Install** | `dotnet add package CrispyWaffle.I18n.PtBr` |
| **Depends on** | `CrispyWaffle` (no third-party package) |

## How it works

All methods remove non-digit characters from the input before working on it, so `(11) 98765-4321`, `11 98765 4321` and `11987654321` are equivalent.

### Phone numbers

`TryParseBrazilianPhoneNumber` applies these rules to the digits:

1. If the digits start with `55` and there are 12 to 14 of them, the `55` country code is removed.
2. If fewer than 10 or more than 12 digits remain, parsing fails.
3. The area code (DDD) is the first two digits, or digits two and three when the number starts with `0` and has 11 or 12 digits (a trunk prefix such as `011`).
4. If the ninth digit from the end is `9`, the number is the last nine digits (mobile number with the extra 9). Otherwise it is the last eight digits.

Rule 4 also applies to 10-digit input, where the ninth digit from the end is the second digit of the area code. A landline in an area code ending in 9 (for example `19 3234-5678`) is therefore parsed as the nine-digit number `932345678`.

The result is a `PhoneNumber` (from the core package, namespace `CrispyWaffle.Utilities`) with `CountryCode` 55, `RegionCode` set to the area code and `Number` set to the subscriber number. `PhoneNumber.IsNinthDigit` is `true` when the number has nine digits.

`ParseBrazilianPhoneNumber` does the same but throws `InvalidTelephoneNumberException` (namespace `CrispyWaffle.GoodPractices`) when parsing fails. The exception message contains the digits only, for example `The value '123456' isn't a valid telephone number.` for the input `abc123def456`.

### Documents and zip codes

`FormatBrazilianDocument` picks the pattern from the length of the input string as given (not of its digits): 14 characters use the CNPJ pattern `00.000.000/0000-00`, any other length uses the CPF pattern `000.000.000-00`. Pass unformatted digits. The digits are converted to a number, so leading zeros are restored by the pattern (`01234567890` becomes `012.345.678-90`).

`FormatBrazilianZipCode` removes non-digits and rewrites every run of eight digits as `00000-000`. For a normal CEP that is the whole value; extra digits are left as they are.

## Getting started

```csharp
using CrispyWaffle.I18n.PtBr;

var phone = "5511987654321".ParseBrazilianPhoneNumber();
// phone.CountryCode == 55, phone.RegionCode == 11, phone.Number == 987654321, phone.IsNinthDigit == true

var cpf = "12345678900".FormatBrazilianDocument();      // "123.456.789-00"
var cnpj = "12345678901234".FormatBrazilianDocument();  // "12.345.678/9012-34"
var cep = "01310100".FormatBrazilianZipCode();          // "01310-100"
```

## Reference

### `ConversionExtensions`

`public static class ConversionExtensions` — extension methods on `string` for Brazilian phone numbers, documents and zip codes.

| Member | Description |
|---|---|
| `ParseBrazilianPhoneNumber(this string number)` | Parses `number` with the rules in How it works and returns a `PhoneNumber`. Throws `InvalidTelephoneNumberException` with the digits of `number` when it is not a valid Brazilian phone number. |
| `TryParseBrazilianPhoneNumber(this string number, ref PhoneNumber result)` | Parses `number` and stores the result in `result`. Returns `true` on success and `false` when the number of digits is invalid; `result` is unchanged on failure. Throws `NullReferenceException` when `number` is `null`. |
| `FormatBrazilianDocument(this string document)` | Formats `document` as a CNPJ (`00.000.000/0000-00`) when the input is 14 characters long, otherwise as a CPF (`000.000.000-00`). Returns `"Invalid document"` when `document` is null, empty or whitespace. |
| `FormatBrazilianZipCode(this string zipCode)` | Removes non-digits and formats each run of eight digits as `00000-000`. Returns `"Invalid zipcode"` when `zipCode` is null, empty or whitespace. Fewer than eight digits are returned unformatted. |

## Examples

### Parse phone numbers in different formats

Outputs match the package's unit tests.

```csharp
using CrispyWaffle.I18n.PtBr;

var a = "5511987654321".ParseBrazilianPhoneNumber(); // 55, 11, 987654321, IsNinthDigit: true
var b = "551187654321".ParseBrazilianPhoneNumber();  // 55, 11, 87654321,  IsNinthDigit: false
var c = "11987654321".ParseBrazilianPhoneNumber();   // 55, 11, 987654321, IsNinthDigit: true
var d = "1187654321".ParseBrazilianPhoneNumber();    // 55, 11, 87654321,  IsNinthDigit: false
```

### Validate user input without exceptions

`TryParseBrazilianPhoneNumber` takes the result by `ref`, so initialize it first.

```csharp
using CrispyWaffle.I18n.PtBr;
using CrispyWaffle.Utilities;

var phone = new PhoneNumber(0, 0, 0);

if ("(11) 98765-4321".TryParseBrazilianPhoneNumber(ref phone))
{
    // phone.ToString() == "5511987654321"
}

var valid = "190".TryParseBrazilianPhoneNumber(ref phone); // false
```

### Handle an invalid phone number

```csharp
using CrispyWaffle.GoodPractices;
using CrispyWaffle.I18n.PtBr;

try
{
    "abc123def456".ParseBrazilianPhoneNumber();
}
catch (InvalidTelephoneNumberException e)
{
    // e.Message == "The value '123456' isn't a valid telephone number."
}
```

### Format documents

```csharp
using CrispyWaffle.I18n.PtBr;

"12345678900".FormatBrazilianDocument();     // "123.456.789-00"
"12345678901234".FormatBrazilianDocument();  // "12.345.678/9012-34"
string.Empty.FormatBrazilianDocument();      // "Invalid document"
```

## See also

- [Utilities](../core/utilities.md)
- [Extensions](../core/extensions.md)
- [Validations](../core/validations.md)

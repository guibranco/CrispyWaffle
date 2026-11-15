# CrispyWaffle.I18n.PtBr

[![NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.I18n.PtBr.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.I18n.PtBr/) [![NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.I18n.PtBr.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.I18n.PtBr/) [![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE)

Part of [Crispy Waffle](https://github.com/guibranco/CrispyWaffle), a toolkit for .NET projects.

CrispyWaffle.I18n.PtBr adds string extension methods for Brazilian data: parsing Brazilian phone numbers and formatting CPF/CNPJ documents and CEP zip codes. Use it when you receive these values as free text (with or without punctuation) and need a normalized value or a display format.

| | |
|---|---|
| **Package** | `CrispyWaffle.I18n.PtBr` |
| **Namespace** | `CrispyWaffle.I18n.PtBr` |
| **Install** | `dotnet add package CrispyWaffle.I18n.PtBr` |
| **Depends on** | `CrispyWaffle` (no third-party package) |

## Supported frameworks

This package targets **.NET Standard 2.0**, **.NET Standard 2.1**, **.NET 8.0**, **.NET 9.0** and **.NET 10.0**.

## Getting started

```csharp
using CrispyWaffle.I18n.PtBr;

var phone = "5511987654321".ParseBrazilianPhoneNumber();
// phone.CountryCode == 55, phone.RegionCode == 11, phone.Number == 987654321, phone.IsNinthDigit == true

var cpf = "12345678900".FormatBrazilianDocument();      // "123.456.789-00"
var cnpj = "12345678901234".FormatBrazilianDocument();  // "12.345.678/9012-34"
var cep = "01310100".FormatBrazilianZipCode();          // "01310-100"
```

## Documentation

The full reference and more examples for this package are on the [CrispyWaffle.I18n.PtBr documentation page](https://guibranco.github.io/CrispyWaffle/packages/i18n-ptbr/).

- [Crispy Waffle documentation](https://guibranco.github.io/CrispyWaffle/)
- [Changelog](https://guibranco.github.io/CrispyWaffle/changelog/)
- [Source code](https://github.com/guibranco/CrispyWaffle)
- [Report an issue](https://github.com/guibranco/CrispyWaffle/issues/new)

## License

Released under the [MIT License](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE).

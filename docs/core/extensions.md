# Extension methods

The `CrispyWaffle.Extensions` namespace holds a set of static helper classes, mostly extension methods, for common chores: converting strings to numbers and dates, cleaning and reshaping text, reading enum attributes, flattening exception chains, rounding integers, reflecting over types and reading XML.

Each class has no state and no dependencies to wire up. Add `using CrispyWaffle.Extensions;` and the methods appear on the types they extend.

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.Extensions` |

## How it works

Most methods are forgiving. They return a neutral value (`0`, `string.Empty`, the original input, or `false`) for null, empty or whitespace input instead of throwing. The exceptions to this rule are listed in each table: for example `ToDateTime` throws on empty input, and `Levenshtein` does not check for null.

A few methods depend on legacy code pages that .NET (Core) does not load by default:

- `RemoveDiacritics` and `LevenshteinInvariantCulture` use code page 1251.
- `ToSafeUrl` uses ISO-8859-8.

On .NET Core and .NET 5 or later, register the code pages provider once at startup before you call these methods. The library does not do it for you.

```csharp
using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
```

Parsing methods (`ToInt32`, `ToInt64`, `ToDecimal`, `ToMonetary`, `TryToDateTime`) use the current culture unless noted.

## Getting started

```csharp
using System;
using CrispyWaffle.Extensions;

var quantity = "1,250".ToInt32();             // 1250 under en-US; 0 if parsing fails
var due = "tomorrow".ToDateTime();            // DateTime.Today.AddDays(1)
var active = "s".ToBoolean();                 // true: matches the default "S", case-insensitive
var title = "republica federativa do brasil".ToCamelCase(); // "Republica Federativa Do Brasil"
var slots = 15.RoundUp();                     // 20
var place = 22.ToOrdinal();                   // "22nd"
```

## Reference

### `ConversionExtensions`

`public static class ConversionExtensions` — conversions between strings, numbers, dates, booleans, bytes, JSON and XML.

| Member | Extends | Description |
|---|---|---|
| `ToBoolean(string validValueForTrue = "S")` | `string` | Returns `true` only when the string equals `validValueForTrue` (case-insensitive). Any other value, including null, returns `false`. The default `"S"` is the Portuguese "Sim". |
| `ToString(string trueValue, string falseValue)` | `bool` | Returns `trueValue` or `falseValue`. |
| `ToDateTime()` | `string` | Parses with the same rules as `TryToDateTime`. Throws `ArgumentNullException` when the input is null or whitespace, and `ArgumentOutOfRangeException` when it cannot be parsed. |
| `TryToDateTime(out DateTime value)` | `string` | Accepts the keywords `now`, `today`, `yesterday` and `tomorrow` (case-insensitive, trimmed), then `DateTime.TryParse` with the current culture, then the exact format `dd/MM/yyyy` for 10-character input. Returns `false` and `DateTime.MinValue` on failure. |
| `ToInt32()` / `ToInt64()` / `ToDecimal()` | `string` | Parses with `NumberStyles.Number` and the current culture. Returns `0` for null, whitespace or unparseable input. |
| `ToBytes()` | `IEnumerable<string>` | Converts each base-2 string (for example `"01000001"`) to a byte. Throws `FormatException` on non-binary input. |
| `ToBinaryString()` | `IEnumerable<byte>` | Converts each byte to an 8-character base-2 string. |
| `ToMonetary(CultureInfo cultureInfo = null)` | `decimal` | Formats with `{0:C}` in `cultureInfo` or the current culture. Returns the literal `"No value"` for `0`. |
| `ToUnixTimeStamp()` | `DateTime` | Converts to UTC and returns the number of seconds since 1970-01-01 as an `int`. |
| `FromUnixTimeStamp()` | `int` | Converts an epoch value to a local `DateTime`. The value is divided by 1000 (and rounded) before it is added as seconds, so it is treated as milliseconds. It is therefore not the inverse of `ToUnixTimeStamp`. |
| `ToPrettyString()` | `string` | Re-formats a JSON object or array with indentation (Newtonsoft.Json). Returns `string.Empty` for null or whitespace and returns the input unchanged when `IsValidJson` is `false`. |
| `ToIdentString()` | `XmlDocument` | Serializes the document with tab indentation and CRLF line breaks. Returns `null` for a null document. |
| `ToModule10()` | `string` | Strips non-digits and computes a modulus-10 check digit with alternating weights 2 and 1 from the right (products of 10 or more have their digits summed). Returns `0`–`9`. |
| `ToOrdinal()` | `int`, `long` | Appends an English ordinal suffix (`1st`, `2nd`, `3rd`, `11th`, `22nd`). Negative numbers are returned without a suffix. |
| `DeepClone<T>(bool useNonPublic = true)` | `T` | Builds a new `T` by calling its public constructor with the most parameters. Each argument comes from a property with the same name (case-insensitive) and exact type: public properties first, then non-public ones when `useNonPublic` is `true`. Property values are passed by reference, so nested objects are shared and the copy is shallow. Returns `default` when `T` has no public constructor. If a constructor parameter has no matching property, the argument list is too short and the constructor call throws. |

### `StringExtensions`

`public static class StringExtensions` — cleaning, formatting, encoding and comparing strings. All methods are marked `[Pure]`.

| Member | Extends | Description |
|---|---|---|
| `RemoveNonNumeric()` | `string` | Removes every character except `0`–`9`. |
| `RemoveNonAlphanumeric()` | `string` | Removes every character that is not a word character (`\w`), `.`, `@` or `-`. |
| `ReplaceNonAlphanumeric(string replaceWith)` | `string` | Like `RemoveNonAlphanumeric`, but replaces each character with `replaceWith`. |
| `RemoveSpaces()` | `string` | Removes all whitespace. |
| `RemoveExcessSpaces()` | `string` | Replaces each run of two or more whitespace characters with a single space. The pattern is `[\t\|\s]{2,}`, so runs of `\|` characters are replaced as well. |
| `RemoveDiacritics()` | `string` | Removes accents by round-tripping through code page 1251 and ASCII. Requires the code pages provider on .NET Core (see [How it works](#how-it-works)). |
| `ReplaceFirst(string search, string replaceWith)` | `string` | Replaces the first ordinal occurrence of `search`. Returns the input unchanged when `search` is not found. |
| `Abbreviate(int maxCharacters, bool addEllipsis = true)` | `string` | Returns the input unchanged when it is shorter than `maxCharacters`. Otherwise truncates to `maxCharacters - 4` characters and appends `"..."` (or to `maxCharacters` characters with no ellipsis). Returns `string.Empty` for null or whitespace. |
| `ToCamelCase()` | `string` | Splits on spaces, lowercases each word and capitalizes its first letter, then joins with single spaces. Despite the name, the result is title case: `"hello WORLD"` becomes `"Hello World"`. Returns `string.Empty` for null or whitespace. |
| `ToUcWords()` | `string` | Lowercases the text, then uppercases each `a`–`z` letter at the start of the text or after whitespace, `/` or a digit. |
| `UcWords(string[] toUpper, string[] toLower)` | `string[]` | Joins the words, each preceded by a space (so the result starts with a space). A word is fully uppercased when its uppercase form is in `toUpper`, lowercased when its lowercase form is in `toLower`, and passed to `ToCamelCase` otherwise. |
| `ToSafeUrl()` | `string` | Builds a URL slug: transcodes through ISO-8859-8, removes `-` and `.`, collapses spaces, replaces remaining non-word characters with `-` and trims `-` from both ends. Case is preserved. Requires the code pages provider on .NET Core. |
| `ToBase64()` | `string` | Encodes the UTF-8 bytes as Base64. Returns `string.Empty` for null or whitespace. |
| `FromBase64(string encoding = "ISO-8859-1")` | `string` | Decodes Base64 and reads the bytes with `encoding`. Because the default is ISO-8859-1 and `ToBase64` uses UTF-8, non-ASCII text does not round-trip unless you pass `"UTF-8"`. |
| `FromISO2UTF8()` | `string` | Encodes the string as ISO-8859-1, converts the bytes to UTF-8 and decodes them. Characters outside ISO-8859-1 become `?`. |
| `ToValidFileName()` | `string` | Replaces runs of characters from `Path.GetInvalidFileNameChars()`, and trailing dots, with `_`. Throws `ArgumentNullException` for null. |
| `Levenshtein(string inputToCompare)` | `string` | Returns the Levenshtein edit distance (case-sensitive). Throws `NullReferenceException` if either string is null. |
| `LevenshteinInvariantCulture(string inputToCompare)` | `string` | Removes diacritics and lowercases both strings, then calls `Levenshtein`. Requires the code pages provider on .NET Core. |
| `GetPathOrFileName()` | `string` | Parses an absolute URL. Returns the file name when the path has an extension, otherwise the absolute path. Throws `UriFormatException` for relative or malformed URLs. |
| `GetFileName()` | `Uri` | Returns `Path.GetFileName` of the local path, or `string.Empty`. |
| `GetFileExtension()` | `Uri`, `string` | Returns the extension including the leading dot (for example `.pdf`), or `string.Empty`. |
| `ToCenter(char spacer, int lineSize)` | `string` | Pads both sides with `lineSize - input.Length / 2` copies of `spacer` (one more on the right for odd lengths). `lineSize` is effectively half the final width: the result is about `2 * lineSize` characters long. |
| `IsValidJson()` | `string` | Returns `true` when the trimmed string starts and ends with `{}` or `[]` and parses with `JToken.Parse`. Returns `false` for null, scalars and parse errors. |
| `SplitBy(int chunkLength)` | `string` | Lazily yields chunks of `chunkLength` characters (the last may be shorter). Yields a single `string.Empty` for null or whitespace input. Throws `ArgumentException` when `chunkLength < 1`; because the method is an iterator, the exception surfaces on first enumeration. |
| `StripTags()` | `string` | Removes everything from each `<` to the next `>`. Returns `string.Empty` for null or whitespace. |

### `EnumExtensions`

`public static class EnumExtensions` — reads and resolves the `HumanReadable` and `InternalValue` attributes on enum fields. See [Attributes](attributes.md).

| Member | Extends | Description |
|---|---|---|
| `GetHumanReadableValue(string flagsSeparator = " \| ")` | `Enum` | Returns the `[HumanReadable]` text of the value, or `null` if the field has none. For a `[Flags]` enum with more than one bit set, joins the text of each set single-bit flag with `flagsSeparator`. In that case every set flag must have the attribute, or an `IndexOutOfRangeException` is thrown. Throws `ArgumentNullException` when a non-flags value does not match a declared field. |
| `GetInternalValue()` | `Enum` | Returns the `[InternalValue]` text of the value, or `null` if the field has none. |
| `GetUniqueFlags<T>()` | `Enum` | Lazily yields each declared single-bit value (as `T`, usually the enum type) that is set in the value. |
| `GetEnumByHumanReadableAttribute<T>(string humanReadableValue)` | — (static) | Returns the field of enum `T` whose `[HumanReadable]` text or field name equals `humanReadableValue` (case-insensitive). Throws `InvalidOperationException` when `T` is not an enum or more than one field matches, and `ArgumentOutOfRangeException` when nothing matches. |
| `GetEnumByInternalValueAttribute<T>(string internalValue)` | — (static) | Same as above, using `[InternalValue]`. Returns `default(T)` when `internalValue` is null. |

### `ExceptionExtensions`

`public static class ExceptionExtensions` — flattens an exception chain and formats it. The logging infrastructure's default exception handler uses these methods; see [Logging](logging.md).

| Member | Extends | Description |
|---|---|---|
| `ToQueue(out List<Type> types)` | `Exception` | Returns a queue holding the exception and all its inner exceptions, with the innermost exception first and the original exception last. `types` receives the types of the inner exceptions only (not the outer one), ordered from outer to inner. |
| `GetMessages(string category, ICollection<ILogProvider> additionalProviders)` | `Queue<Exception>` | Dequeues every exception (the queue is empty afterwards) and builds one string with each message, the exception type name in brackets, and the stack trace. Entries after the first are prefixed with `Exception rethrow at [n]:` and a space, where `n` is the entry's position. For each exception, also calls `Error(category, message)` on every provider in `additionalProviders`, which must not be null. |

### `MathExtensions`

`public static class MathExtensions` — rounds integers to a multiple.

| Member | Extends | Description |
|---|---|---|
| `RoundDown(int multipleOf = 10)` | `int` | Rounds down to the nearest multiple (`15` becomes `10`). Negative values round toward zero. |
| `RoundUp(int multipleOf = 10, bool forceDifferentValue = false)` | `int` | Rounds up to the next multiple (`11` becomes `20`). An exact multiple is returned unchanged unless `forceDifferentValue` is `true`, in which case `multipleOf` is added. |
| `RoundBest(int multipleOf = 10, bool forceDifferentValue = false)` | `int` | Rounds to the nearest multiple: down when the remainder is less than `multipleOf / 2`, up otherwise. A remainder of exactly `1` always rounds down. An exact multiple is returned unchanged even when `forceDifferentValue` is `true`. |

### `TypeExtensions`

`public static class TypeExtensions` — reflection helpers.

| Member | Extends | Description |
|---|---|---|
| `Implements<TInterface>()` | `Type` | Returns `true` when `TInterface` is assignable from the type. `TInterface` must be a reference type. |
| `IsSimpleType()` | `Type` | Returns `true` for value types, primitives, `string`, `decimal`, `DateTime`, `DateTimeOffset`, `TimeSpan`, `Guid`, and any type whose `TypeCode` is not `Object`. |
| `IsNumericType()` | `Type` | Returns `true` for `byte`, `sbyte`, the 16-, 32- and 64-bit signed and unsigned integers, `decimal`, `double` and `float`, and for `Nullable<T>` of any of these. |
| `UpdateValues<T>(T newObject)` | `T : IUpdateable` | Copies values from `newObject` into the current object and returns the current object. A property is copied when its new value is non-null and differs from the value on a freshly constructed `T`, or when `newObject` has a `bool ShouldSerialize<PropertyName>()` method that returns `true`. Simple types are assigned directly; properties whose type implements `IUpdateable` are merged recursively; other reference types (such as collections) are skipped. `T` needs a public parameterless constructor and settable properties. |
| `GetQueryStringBuilderKeyName()` | `PropertyInfo` | Returns the key from `[QueryStringBuilderKeyName]`, or `null`. Used by `QueryStringBuilder.AddFromType`. |
| `QueryStringBuilderIgnore()` | `PropertyInfo` | Returns `true` when the property has `[QueryStringBuilderIgnore]`. |
| `GetCallingMethod(int frame = 1, bool excludeBegin = true)` | — (static) | Returns `Namespace.Type.Method` for the given stack frame (`1` is the method that called `GetCallingMethod`). With `excludeBegin`, a leading `CrispyWaffle.` is removed. Returns `"CrispyWaffle"` when the frame has no method. |

### `XmlExtensions`

`public static class XmlExtensions` — helpers for `XmlReader`, `XmlDocument` and `XmlNode`.

| Member | Extends | Description |
|---|---|---|
| `DuplicateReaders(out XmlReader cloneOne, out XmlReader cloneTwo)` | `XmlReader` | Loads the reader into an `XmlDocument`, writes it to a `MemoryStream` and creates two readers (with `IgnoreWhitespace`) over that stream, then closes the original reader. Both clones share the same stream, so they are not independent: read one to the end before you start the other, and expect problems with documents larger than the reader buffer. |
| `GetFirstElementByTagName(string tagName)` | `XmlDocument` | Returns the first element with the tag name, or `null`. |
| `GetNodeValue(string node)` | `XmlNode` | Evaluates `node` as an XPath expression against the node and returns the inner text of the first match, or `string.Empty`. |
| `GetNodeValue(string[] nodes)` | `XmlNode` | Evaluates each expression and joins the inner texts with a space. Missing nodes contribute an empty string. |

## Examples

### Parse user input safely

`ToInt32` and `TryToDateTime` never throw, so they suit form and CSV input.

```csharp
using System;
using CrispyWaffle.Extensions;

var page = "abc".ToInt32();                    // 0
if ("31/12/2020".TryToDateTime(out var date))  // dd/MM/yyyy fallback
{
    Console.WriteLine(date.ToString("yyyy-MM-dd")); // 2020-12-31
}

Console.WriteLine(1000m.ToMonetary(new System.Globalization.CultureInfo("en-US"))); // $1,000.00
```

### Map enums to display and storage values

```csharp
using CrispyWaffle.Attributes;
using CrispyWaffle.Extensions;

var label = OrderStatus.Pending.GetHumanReadableValue();   // "Waiting for payment"
var code = OrderStatus.Shipped.GetInternalValue();         // "SH"
var status = EnumExtensions.GetEnumByInternalValueAttribute<OrderStatus>("sh"); // OrderStatus.Shipped

public enum OrderStatus
{
    [HumanReadable("Waiting for payment")]
    [InternalValue("WP")]
    Pending = 0,

    [HumanReadable("Shipped")]
    [InternalValue("SH")]
    Shipped = 1,
}
```

### Flatten an exception chain

```csharp
using System;
using System.Collections.Generic;
using CrispyWaffle.Extensions;
using CrispyWaffle.Log.Providers;

try
{
    throw new InvalidOperationException("Outer", new ArgumentException("Inner"));
}
catch (Exception ex)
{
    var queue = ex.ToQueue(out var innerTypes);   // Inner first, then Outer
    var text = queue.GetMessages("Orders", new List<ILogProvider>());
    Console.WriteLine(text);
}
```

### Compare strings ignoring accents and case

```csharp
using System.Text;
using CrispyWaffle.Extensions;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var distance = "Three".LevenshteinInvariantCulture("tree"); // 1
```

## See also

- [Attributes](attributes.md) for `HumanReadable`, `InternalValue` and the query string attributes
- [Utilities](utilities.md) for `QueryStringBuilder` and `IUpdateable`
- [Validations](validations.md) for the regular expressions used by the string helpers
- [pt-BR internationalization](../packages/i18n-ptbr.md) for Brazilian-specific conversions

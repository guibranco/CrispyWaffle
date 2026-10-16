# Attributes

The `CrispyWaffle.Attributes` namespace contains four small attributes that attach metadata to enum fields and to properties. They do nothing on their own; other parts of the library read them:

- `HumanReadableAttribute` and `InternalValueAttribute` give enum fields a display text and an external code. The enum helpers in `EnumExtensions` read them (see [Extensions](extensions.md)).
- `QueryStringBuilderIgnoreAttribute` and `QueryStringBuilderKeyNameAttribute` control how `QueryStringBuilder.AddFromType` turns an object into query string parameters (see [Utilities](utilities.md)).

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.Attributes` |

## How it works

### Enum attributes

Both enum attributes are restricted to fields (`AttributeTargets.Field`), so they are applied to enum members. The core library uses `HumanReadable` on its own enums, such as `Lifetime` and `LogLevel`.

| Reader in `CrispyWaffle.Extensions.EnumExtensions` | Behavior |
|---|---|
| `GetHumanReadableValue(this Enum value, string flagsSeparator = " \| ")` | Returns the field's `HumanReadable` text, or `null` when the field has none. For a `[Flags]` value with more than one flag set, returns the texts of every set flag joined by `flagsSeparator`, or `"None"` when no non-zero flag matches. In that case every set flag must have the attribute, or an `IndexOutOfRangeException` is thrown. |
| `GetEnumByHumanReadableAttribute<T>(string humanReadableValue)` | Returns the field whose `HumanReadable` text or field name equals the value, ignoring case. Throws `InvalidOperationException` when `T` is not an enum and `ArgumentOutOfRangeException` when nothing matches. |
| `GetInternalValue(this Enum value)` | Returns the field's `InternalValue` text, or `null` when the field has none. |
| `GetEnumByInternalValueAttribute<T>(string internalValue)` | Returns the field whose `InternalValue` text or field name equals the value, ignoring case. Returns `default(T)` for `null`. Throws `InvalidOperationException` when `T` is not an enum and `ArgumentOutOfRangeException` when nothing matches. |

`HumanReadableAttribute` is marked `[Localizable(true)]` and is meant for text shown to users. `InternalValueAttribute` takes a `[Localizable(false)]` value and is meant for codes exchanged with other systems (database values, API codes, file formats).

### Query string attributes

Both query string attributes are restricted to properties (`AttributeTargets.Property`). `QueryStringBuilder.AddFromType<T>(T instance, bool convertCamelCaseToUnderscore = true)` walks the public properties of `T`:

1. A property marked with `QueryStringBuilderIgnore` is skipped.
2. The parameter name is the `QueryStringBuilderKeyName` value when present, otherwise the property name.
3. When `convertCamelCaseToUnderscore` is `true` (the default), every upper-case letter after the first character is replaced with `_` plus the lower-case letter. This also applies to names set with `QueryStringBuilderKeyName`, so `[QueryStringBuilderKeyName("pageSize")]` produces `page_size`.
4. A name that starts with the type name is rewritten as `typename[rest]`.
5. The value is the property's `ToString()` result, or an empty string for `null`.

## Getting started

```csharp
using System;
using CrispyWaffle.Attributes;
using CrispyWaffle.Extensions;

var status = OrderStatus.Shipped;

Console.WriteLine(status.GetHumanReadableValue()); // Shipped to customer
Console.WriteLine(status.GetInternalValue()); // SHP

var fromCode = EnumExtensions.GetEnumByInternalValueAttribute<OrderStatus>("pnd");
Console.WriteLine(fromCode); // Pending

public enum OrderStatus
{
    [HumanReadable("Waiting for payment")]
    [InternalValue("PND")]
    Pending,

    [HumanReadable("Shipped to customer")]
    [InternalValue("SHP")]
    Shipped,
}
```

## Reference

### `HumanReadableAttribute`

`[AttributeUsage(AttributeTargets.Field)] public sealed class HumanReadableAttribute : Attribute` — the display text of an enum field.

| Member | Description |
|---|---|
| `HumanReadableAttribute(string value)` | Constructor. `value` is the display text. |
| `StringValue` | The display text passed to the constructor. |

### `InternalValueAttribute`

`[AttributeUsage(AttributeTargets.Field)] public sealed class InternalValueAttribute : Attribute` — an internal or external code for an enum field.

| Member | Description |
|---|---|
| `InternalValueAttribute(string value)` | Constructor. `value` is the code. |
| `InternalValue` | The code passed to the constructor. |

### `QueryStringBuilderIgnoreAttribute`

`[AttributeUsage(AttributeTargets.Property)] public sealed class QueryStringBuilderIgnoreAttribute : Attribute` — excludes a property from `QueryStringBuilder.AddFromType`.

It has no members. The presence of the attribute is checked with the `PropertyInfo.QueryStringBuilderIgnore()` extension method in `CrispyWaffle.Extensions.TypeExtensions`.

### `QueryStringBuilderKeyNameAttribute`

`[AttributeUsage(AttributeTargets.Property)] public sealed class QueryStringBuilderKeyNameAttribute : Attribute` — sets the parameter name used for a property by `QueryStringBuilder.AddFromType`.

The value is read with the `PropertyInfo.GetQueryStringBuilderKeyName()` extension method in `CrispyWaffle.Extensions.TypeExtensions`, which returns `null` when the attribute is absent.

| Member | Description |
|---|---|
| `QueryStringBuilderKeyNameAttribute(string keyName)` | Constructor. `keyName` is the parameter name. |
| `KeyName` | The parameter name passed to the constructor. |

## Examples

### Parse user input into an enum

Matching is case-insensitive and also accepts the field name. This pattern comes from the test suite.

```csharp
using System;
using CrispyWaffle.Attributes;
using CrispyWaffle.Extensions;

var a = EnumExtensions.GetEnumByHumanReadableAttribute<TestEnum>("Três"); // TestEnum.Three
var b = EnumExtensions.GetEnumByHumanReadableAttribute<TestEnum>("três"); // TestEnum.Three
var c = EnumExtensions.GetEnumByHumanReadableAttribute<TestEnum>("Two"); // TestEnum.Two (field name)

Console.WriteLine(TestEnum.Two.GetHumanReadableValue()); // Dois

public enum TestEnum
{
    [InternalValue("ноль")]
    [HumanReadable("Zero")]
    None = 0,

    [InternalValue("один")]
    [HumanReadable("Um")]
    One = 1,

    [InternalValue("два")]
    [HumanReadable("Dois")]
    Two = 2,

    [InternalValue("три")]
    [HumanReadable("Três")]
    Three = 3,
}
```

### Build a query string from an object

Rename one property and skip another.

```csharp
using System;
using CrispyWaffle.Attributes;
using CrispyWaffle.Utilities;

var filter = new SearchFilter
{
    Text = "waffles",
    Page = 2,
    CacheKey = "not-sent",
};

var query = new QueryStringBuilder().AddFromType(filter).ToString();
Console.WriteLine(query); // ?q=waffles&Page=2

public sealed class SearchFilter
{
    [QueryStringBuilderKeyName("q")]
    public string Text { get; set; }

    public int Page { get; set; }

    [QueryStringBuilderIgnore]
    public string CacheKey { get; set; }
}
```

## See also

- [Extensions](extensions.md)
- [Utilities](utilities.md)
- [Service Locator](service-locator.md)

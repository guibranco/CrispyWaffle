# Utilities

The `CrispyWaffle.Utilities` namespace collects small, self-contained building blocks: a chainable query string builder, a lazy value that can be reset and reports load statistics, a dynamic object that serializes to XML or JSON, a phone number value type, and a few contracts for API responses and partial updates.

None of these types need registration. Create them where you need them.

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.Utilities` |

## Getting started

```csharp
using System;
using CrispyWaffle.Utilities;

// Build a query string
var query = new QueryStringBuilder()
    .Add("id", 179)
    .Add("q", "crispy waffle")
    .ToString();                       // "?id=179&q=crispy+waffle"

// Cache an expensive value and reload it on demand
var settings = new LazyResettable<DateTime>(() => DateTime.UtcNow);
var first = settings.Value;            // runs the factory
settings.Reset();
var second = settings.Value;           // runs the factory again
```

## Reference

### `QueryStringBuilder`

`public class QueryStringBuilder : NameValueCollection` — a chainable builder and parser for URL query strings. Marked `[Serializable]`.

Values are URL-encoded with `HttpUtility.UrlEncode` when added and decoded by the indexers. Adding a second value to an existing key stores both, comma-separated, and `ToString()` writes the key once per value.

| Member | Description |
|---|---|
| `QueryStringBuilder()` | Creates an empty builder. |
| `QueryStringBuilder(string queryString)` | Parses a query string or a full URL (everything after the first `?`). Pairs are split on `&` and `=`; values are stored as given, without re-encoding. A pair with no `=`, or with more than one `=`, gets an empty value. |
| `this[string name]` / `this[int index]` | Returns the URL-decoded value. Hides the base indexers and is read-only. |
| `Add(string name, string value, bool isUnique = false)` | URL-encodes `value` and adds it. If the key already has a value, `isUnique: true` replaces it; otherwise the new value is appended after a comma. Adding an empty value to a key that has no value (without `isUnique`) stores a bare `,`, so `ToString()` writes the key twice with empty values. Throws `ArgumentNullException` when `name` or `value` is null. Returns the builder. |
| `Add(string name, object value, bool isUnique = false)` | Calls `value.ToString()` and forwards to the string overload. |
| `AddRange(Dictionary<string, string> items, bool isUnique = false)` | Calls `Add` for each entry. Returns the builder. |
| `AddFromType<T>(T instance, bool convertCamelCaseToUnderscore = true)` | Adds one entry per public property of `T` (the static type). Properties with `[QueryStringBuilderIgnore]` are skipped, and `[QueryStringBuilderKeyName]` overrides the key. With `convertCamelCaseToUnderscore`, every uppercase letter after the first character is lowercased and prefixed with `_` (`FirstName` becomes `First_name`; the first character keeps its case). A key that starts with the type name (case-insensitive) becomes `typename[rest]`. Null property values are added as empty strings. Returns the builder. |
| `Remove(string name)` | Removes the key if it has a non-empty value. Returns the builder. |
| `Reset()` | Clears all entries. Returns the builder. |
| `Contains(string name)` | Returns `true` when the key exists with a non-whitespace value. |
| `ToString()` | Returns the query string with a leading `?`, for example `?a=1&a=2&b=3`. Returns `string.Empty` when there are no entries. Entries with a null or whitespace key are skipped. |
| `ToString(bool includeNullFields)` | Same, but includes entries whose key is null or whitespace when `includeNullFields` is `true`. The flag looks at keys, not values. |

### `LazyResettable<T>`

`public class LazyResettable<T> : ILazyResettable` — a lazily created value, like `Lazy<T>`, that can be discarded with `Reset()` and recreated on the next access. It counts loads, hits and resets and measures the time spent in the factory. Based on a well-known Stack Overflow answer.

The `mode` argument controls concurrency:

- `PublicationOnly` (default): the factory runs outside the lock. Concurrent callers may each run it; the first result stored wins. Every run counts as a load.
- `ExecutionAndPublication`: the factory runs inside a lock, so only one caller runs it.
- `None`: no locking. Use it only from a single thread.

`Hits` counts accesses that find a value already created. The access that creates the value is a load, not a hit.

| Member | Description |
|---|---|
| `LazyResettable(Func<T> valueFactory, LazyThreadSafetyMode mode = LazyThreadSafetyMode.PublicationOnly, Type declaringType = null)` | Constructor. Throws `ArgumentNullException` when `valueFactory` is null. `declaringType` defaults to the type that declares the factory method. |
| `Value` | Returns the value, running the factory if there is no current value. |
| `IsValueCreated` | `true` when a value is currently held. |
| `Load()` | Reads `Value`, forcing creation. |
| `Reset()` | Discards the current value, increments `Resets` and raises `OnReset`. The next access runs the factory again. |
| `OnReset` | `event EventHandler` raised after every `Reset()`. |
| `DeclaringType` | The type passed to the constructor, or the factory's declaring type. |
| `Stats()` | Returns a new `ResetLazyStats` snapshot with `Type` set to `typeof(T)`. |
| `Loads`, `Hits`, `Resets` | Public `int` fields with the running counters. |
| `SumLoadTime` | Public `TimeSpan` field with the total time spent in the factory. It is not updated atomically. |

### `DynamicSerialization`

`public class DynamicSerialization : DynamicObject, ISerializable, IXmlSerializable, IEquatable<DynamicSerialization>` — a dictionary-backed dynamic object that you fill at run time and serialize as XML or through `ISerializable`.

The class is marked `[Serializable]` and `[Serializer]` (XML format by default), so the [serialization](serialization.md) helpers can handle it. The key filter (`DynamicSerializationOption`) changes key names only on output (`GetObjectData` and `WriteXml`). Stored keys and `GetDictionary()` keep the original names.

| Member | Description |
|---|---|
| `DynamicSerialization()` | Creates an instance with `DynamicSerializationOption.None`. |
| `DynamicSerialization(DynamicSerializationOption keyFilter)` | Creates an instance that transforms keys on output. |
| `DynamicSerialization(SerializationInfo info, StreamingContext context)` | Deserialization constructor. Copies every entry into the dictionary and uses `None`. |
| `SetMember(string key, object value)` | Adds an entry. Throws `DynamicSerializationException` when the key is null or already exists. |
| `TryGetMember(GetMemberBinder binder, out object result)` | Dynamic property read. Returns `null` for unknown members instead of failing. |
| `TrySetMember(SetMemberBinder binder, object value)` | Dynamic property write. Adds the entry; assigning the same member twice throws `ArgumentException`. |
| `GetDictionary()` | Returns the underlying `Dictionary<string, object>` (not a copy). |
| `GetObjectData(SerializationInfo info, StreamingContext context)` | `virtual`. Writes each entry with its filtered key. |
| `GetSchema()` | Returns `null`, as required by `IXmlSerializable`. |
| `ReadXml(XmlReader reader)` | Reads each child element of the current element into the dictionary as a string value keyed by its local name. |
| `WriteXml(XmlWriter writer)` | Writes each entry as an element named by its filtered key, using `XmlWriter.WriteValue`. |
| `Equals(DynamicSerialization other)` / `Equals(object obj)` / `==` / `!=` | Equal when both are the same runtime type, the dictionaries contain the same entries in the same order, and the key filters match. |
| `GetHashCode()` | Combines the dictionary reference hash with the filter. Two equal instances can have different hash codes, so do not use instances as dictionary keys. |
| `Dictionary` | `protected readonly Dictionary<string, object>` holding the entries. |
| `SerializationKeyFilter` | `protected readonly DynamicSerializationOption` set by the constructor. |

### `PhoneNumber`

`public struct PhoneNumber` — a phone number split into country code, region (area) code and local number.

Use `ParseBrazilianPhoneNumber` from the [pt-BR package](../packages/i18n-ptbr.md) to build one from text.

| Member | Description |
|---|---|
| `PhoneNumber(int countryCode, int regionCode, long number)` | Constructor. |
| `CountryCode` | Country code, for example `55`. |
| `RegionCode` | Region or area code, for example `11`. |
| `Number` | Local number without country or region codes. |
| `IsNinthDigit` | `true` when `CountryCode` is `55` and `Number` has nine or more digits (greater than `99999999`), as with Brazilian mobile numbers. |
| `ToString()` | Concatenates the three parts with no separators, for example `5511123456789`. |

### `ILazyResettable`

`public interface ILazyResettable` — the non-generic contract implemented by `LazyResettable<T>`, useful for keeping a list of resettable values of different types.

| Member | Description |
|---|---|
| `Reset()` | Discards the current value. |
| `Load()` | Forces the value to be created. |
| `DeclaringType` | The type that owns the lazy value. |
| `Stats()` | Returns a `ResetLazyStats` snapshot. |

### `IJsonResponse`

`public interface IJsonResponse` — a minimal shape for API responses.

| Member | Description |
|---|---|
| `Code` | `int` status code, typically an HTTP status. |
| `ErrorMessage` | `string` error description. |

### `IUpdateable`

`public interface IUpdateable` — an empty marker interface. Implement it to enable `UpdateValues<T>` from [TypeExtensions](extensions.md#typeextensions), which merges non-default property values from one instance into another.

### `ErrorResponse`

`public class ErrorResponse : IJsonResponse` — a standard error payload.

| Member | Description |
|---|---|
| `ErrorList` | `Dictionary<string, IEnumerable<string>>` of messages keyed by field or context. Not initialized by default. |
| `Code` | `int` error code. |
| `ErrorMessage` | `string` general error message. |

### `ResetLazyStats`

`public class ResetLazyStats` — a snapshot of the counters of a `LazyResettable<T>`.

| Member | Description |
|---|---|
| `ResetLazyStats(Type type)` | Constructor. Sets `Type`; counters start at zero. |
| `Type` | Public field. The value type being tracked. |
| `Loads` | Public `int` field. Number of factory runs. |
| `Hits` | Public `int` field. Number of reads that found a value already created. |
| `Resets` | Public `int` field. Number of `Reset()` calls. |
| `SumLoadTime` | Public `TimeSpan` field. Total time spent in the factory. |

### `DynamicSerializationOption`

`public enum DynamicSerializationOption` — how `DynamicSerialization` transforms keys on output.

| Value | Description |
|---|---|
| `None` | Keys are written unchanged. |
| `Lowercase` | Keys are lowercased with `ToLowerInvariant`. |
| `Uppercase` | Keys are uppercased with `ToUpperInvariant`. |
| `Camelcase` | Keys are passed through `StringExtensions.ToCamelCase`, which actually produces title case: `"userName"` becomes `"Username"` and `"user name"` becomes `"User Name"`. |

### `DynamicSerializationException`

`public class DynamicSerializationException : Exception` — thrown by `DynamicSerialization.SetMember` when an entry cannot be added (null or duplicate key). Marked `[Serializable]`.

| Member | Description |
|---|---|
| `DynamicSerializationException(string key, object value, Exception innerException)` | Message: `Could not add key {key} with value {value}`. The original exception is kept as `InnerException`. |
| `DynamicSerializationException(SerializationInfo info, StreamingContext context)` | `protected` deserialization constructor. |

## Examples

### Build a query string from an object

```csharp
using CrispyWaffle.Attributes;
using CrispyWaffle.Utilities;

var filter = new SearchFilter { Term = "waffle", PageSize = 20, SortBy = "name", Secret = "x" };

var query = new QueryStringBuilder()
    .AddFromType(filter)
    .ToString();                      // "?Term=waffle&Page_size=20&sort=name" (Secret is skipped)

public class SearchFilter
{
    public string Term { get; set; }

    public int PageSize { get; set; }

    [QueryStringBuilderKeyName("sort")]
    public string SortBy { get; set; }

    [QueryStringBuilderIgnore]
    public string Secret { get; set; }
}
```

Avoid null or empty property values: they go through the empty-value path of `Add` described in the reference table.

### Parse and modify an existing query string

```csharp
using CrispyWaffle.Utilities;

var builder = new QueryStringBuilder("https://example.com/search?page=1&tag=a");

builder.Add("page", "2", isUnique: true)   // replaces page=1
       .Add("tag", "b");                   // keeps tag=a and adds tag=b

var page = builder["page"];                // "2"
var query = builder.ToString();            // "?page=2&tag=a&tag=b"
```

### Track a resettable cache

```csharp
using System;
using System.Threading;
using CrispyWaffle.Utilities;

var rates = new LazyResettable<decimal>(LoadRate, LazyThreadSafetyMode.ExecutionAndPublication);
rates.OnReset += (_, _) => Console.WriteLine("Rate cache cleared");

var a = rates.Value;   // load
var b = rates.Value;   // hit
rates.Reset();
var c = rates.Value;   // load

var stats = rates.Stats();
Console.WriteLine($"{stats.Loads} loads, {stats.Hits} hits, {stats.Resets} resets"); // 2 loads, 1 hits, 1 resets

static decimal LoadRate() => 5.25m;
```

### Build a dynamic payload

```csharp
using CrispyWaffle.Utilities;

dynamic payload = new DynamicSerialization(DynamicSerializationOption.Lowercase);
payload.OrderId = 42;
payload.Status = "Shipped";

var dictionary = ((DynamicSerialization)payload).GetDictionary(); // keys "OrderId", "Status"
```

When `payload` is serialized, the keys are written as `orderid` and `status`.

## See also

- [Extension methods](extensions.md) for `UpdateValues` and the query string attribute helpers
- [Attributes](attributes.md) for `QueryStringBuilderKeyName` and `QueryStringBuilderIgnore`
- [Serialization](serialization.md)
- [pt-BR internationalization](../packages/i18n-ptbr.md) for parsing Brazilian phone numbers

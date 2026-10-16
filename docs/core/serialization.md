# Serialization

The serialization module converts objects to and from JSON, XML and YAML. You mark a class with `[Serializer]` to pick a format, then call `GetSerializer()` on an instance to get a `SerializerConverter<T>`. The converter can turn the object into a string, an `XmlDocument`, a `JToken` or a stream, and it can deserialize, load from a file or save to a file.

You can also use the adapters (`NewtonsoftJsonSerializerAdapter`, `JsonSerializerAdapter`, `XmlSerializerAdapter`, `YamlSerializerAdapter`) on their own, without the attribute or the factory.

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.Serialization`, `CrispyWaffle.Serialization.Adapters`, `CrispyWaffle.Serialization.NewtonsoftJson`, `CrispyWaffle.Serialization.SystemTextJson` |

## How it works

`SerializerFactory.GetSerializer(obj)` reads the `SerializerAttribute` from the runtime type of `obj` and wraps the object in a `SerializerConverter<T>` with an adapter for that format:

| Attribute | Adapter used |
|---|---|
| `[Serializer]` or `[Serializer(SerializerFormat.Xml)]` | `XmlSerializerAdapter`, resolved through `ServiceLocator.Resolve<XmlSerializerAdapter>()` |
| `[Serializer(SerializerFormat.Json)]` (strict, the default) | `NewtonsoftJsonSerializerAdapter`, resolved through `ServiceLocator.Resolve<NewtonsoftJsonSerializerAdapter>()` |
| `[Serializer(SerializerFormat.Json, false)]` (not strict) | A new `NewtonsoftJsonSerializerAdapter` with `MissingMemberHandling.Ignore` |
| `[Serializer(SerializerFormat.Yaml)]` | `YamlSerializerAdapter`, resolved through `ServiceLocator.Resolve<YamlSerializerAdapter>()` |
| `SerializerFormat.None` | Throws `InvalidOperationException` |

The factory always uses Newtonsoft.Json for JSON. The System.Text.Json adapter (`JsonSerializerAdapter`) is only used if you create it yourself.

Attribute lookup rules:

- The attribute is read from the object's runtime type. `SerializerAttribute` is declared with `Inherited = false`, so a derived class needs its own `[Serializer]`.
- For an array, the attribute is read from the element type (`Customer[]` uses the attribute on `Customer`).
- For any other `IEnumerable`, the attribute is read from the first generic argument (`List<Customer>` uses the attribute on `Customer`).
- If no attribute is found, `GetSerializer` throws `InvalidOperationException` with the message `The CrispyWaffle.Serialization.SerializerAttribute attribute was not found in the object of type ...`.

Strict and non-strict JSON differ only when you deserialize. Strict mode uses `MissingMemberHandling.Error`, so a JSON property with no matching .NET member throws. Non-strict mode uses `MissingMemberHandling.Ignore`. Both modes use the `pt-BR` culture, `NullValueHandling.Ignore` and the Newtonsoft `NotNullObserverConverter`.

`XmlSerializerAdapter`, `NewtonsoftJsonSerializerAdapter` and `YamlSerializerAdapter` have no constructor dependencies, so the service locator can create them without registration.

### Not-null observer

`NotNullObserver` is an empty marker class. Use it as the type of a property that you expect to always be `null` or empty in incoming JSON, such as a field a third-party API documents but never fills. Both JSON adapters register a matching converter by default. When the JSON has a real value for that property, the converter throws `NotNullObserverException`, and the adapter wraps it in a second `NotNullObserverException` that names the target type. This lets you detect when the upstream payload changes.

The two converters accept different "empty" values:

| JSON value | Newtonsoft converter | System.Text.Json converter |
|---|---|---|
| `null` | Returns `null` | Returns `null` |
| `""` or whitespace string | Returns `null` | Returns `null` |
| `[]` | Returns `null` | Throws `JsonException` (see note) |
| `{}` | Returns `null` | Returns `null` |
| Non-empty object | Throws `NotNullObserverException` | Throws `NotNullObserverException` |
| Non-empty string, number, boolean, non-empty array | Throws `NotNullObserverException` | Throws `JsonException` (see note) |

Note: the System.Text.Json converter calls `JsonElement.EnumerateObject()` before it checks the value kind. That call throws `InvalidOperationException` for any value that is not an object, and System.Text.Json wraps it in a `JsonException` ("The JSON value could not be converted to CrispyWaffle.Serialization.NotNullObserver"). In those cases you get `JsonException` instead of `NotNullObserverException`.

## Getting started

```csharp
using System;
using CrispyWaffle.Serialization;

var customer = new Customer { Id = 1, Name = "Jane Doe" };

// Serialize to a JSON string (Newtonsoft.Json, indented).
var json = (string)customer.GetSerializer();

// Deserialize back into a new instance.
Customer copy = SerializerFactory.GetSerializer<Customer>().Deserialize(json);

Console.WriteLine(copy.Name);

[Serializer(SerializerFormat.Json)]
public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; }
}
```

## Reference

### `SerializerFactory`

`public static class SerializerFactory` — creates `SerializerConverter<T>` instances from the `[Serializer]` attribute or from an explicit format.

| Member | Description |
|---|---|
| `GetSerializer<T>(this T obj)` | Extension method. Reads the `SerializerAttribute` from the runtime type of `obj` (or its element / first generic argument for collections) and returns a converter that wraps `obj`. Throws `InvalidOperationException` when no attribute is found or the format is `None`. `T : class`. |
| `GetSerializer<T>()` | Creates a new `T` with `Activator.CreateInstance<T>()` and returns a converter for it. Use it when you only want to deserialize. `T` needs a public parameterless constructor. |
| `GetCustomSerializer<T>(this T obj, SerializerFormat format)` | Extension method. Ignores any attribute and returns a converter for `format`. JSON is always strict here. Throws `InvalidOperationException` for `SerializerFormat.None`. |
| `GetCustomSerializer<T>(SerializerFormat format)` | Same as above, for a new `T` created with `Activator.CreateInstance<T>()`. |

### `SerializerConverter<T>`

`public sealed class SerializerConverter<T> where T : class` — wraps an object and an `ISerializerAdapter`, and exposes conversion operators and file helpers.

Methods that deserialize or load replace the wrapped object and return it.

| Member | Description |
|---|---|
| `SerializerConverter(T obj, ISerializerAdapter formatter)` | Constructor. Wraps `obj` with the given adapter. |
| `Serialize(out Stream stream)` | Serializes the wrapped object with the adapter. The stream is positioned at 0. |
| `Deserialize(object deserialized)` | Deserializes `deserialized` with the adapter, stores the result and returns it. JSON adapters expect a `string`. The XML adapter accepts a `string` or an `XmlDocument`. |
| `DeserializeFromStream(Stream stream, Encoding encoding = null)` | Deserializes from a stream (UTF-8 when `encoding` is `null`), stores the result and returns it. |
| `Load(string file)` | Reads the file as UTF-8, deserializes it, stores the result and returns it. See `BaseSerializerAdapter.Load<T>` for exceptions. |
| `Save(string file)` | Serializes the wrapped object to `file`, replacing it if it exists. |
| `implicit operator XmlDocument(SerializerConverter<T>)` | Serializes to an `XmlDocument`. Returns `null` when the converter is `null` or its adapter is not `XmlSerializerAdapter`. An `InvalidOperationException` during serialization is logged through `LogConsumer.Handle` and an empty document is returned. |
| `implicit operator JToken(SerializerConverter<T>)` | Serializes to a Newtonsoft `JToken`: a `JArray` when the object is `IEnumerable`, otherwise a `JObject`. Returns `null` when the adapter is not `NewtonsoftJsonSerializerAdapter`, or after logging an `InvalidOperationException`. |
| `explicit operator string(SerializerConverter<T>)` | XML adapter: indented XML without the XML declaration. Newtonsoft adapter: indented JSON. Any `IStringSerializerAdapter` (such as `YamlSerializerAdapter`): the result of `SerializeToString`. Throws `InvalidOperationException` for any other adapter (including `JsonSerializerAdapter`). |
| `implicit operator SerializerConverter<T>(XmlDocument xml)` | Creates a converter with the XML adapter and deserializes `xml.InnerXml` into `T`. |
| `implicit operator SerializerConverter<T>(JObject json)` | Creates a converter with the Newtonsoft adapter and deserializes `json` into `T`. |
| `implicit operator T(SerializerConverter<T>)` | Returns the wrapped object. |

### `ISerializerAdapter`

`public interface ISerializerAdapter` — contract for a format-specific serializer. All methods have the constraint `T : class`.

| Member | Description |
|---|---|
| `DeserializeFromStream<T>(Stream stream, Encoding encoding = null)` | Deserializes a stream. `null` encoding means UTF-8. |
| `Deserialize<T>(object serialized)` | Deserializes a serialized representation (usually a `string`). |
| `Load<T>(string file)` | Reads a file and deserializes its content. |
| `Serialize<T>(T deserialized, out Stream stream)` | Serializes an object into a new stream. |
| `Save<T>(string file, T deserialized)` | Serializes an object and writes it to a file. |

### `IStringSerializerAdapter`

`public interface IStringSerializerAdapter : ISerializerAdapter` — an adapter that can serialize directly to a string. `SerializerConverter<T>` uses it for the explicit `string` conversion.

| Member | Description |
|---|---|
| `SerializeToString<T>(T deserialized)` | Serializes an object to its string representation. `T : class`. |

### `BaseSerializerAdapter`

`public abstract class BaseSerializerAdapter : ISerializerAdapter` — base class with file handling. Subclass it to add a format.

| Member | Description |
|---|---|
| `virtual DeserializeFromStream<T>(Stream stream, Encoding encoding = null)` | Throws `NotImplementedException`. Override it. |
| `virtual Deserialize<T>(object serialized)` | Throws `NotImplementedException`. Override it. |
| `virtual Serialize<T>(T deserialized, out Stream stream)` | Throws `NotImplementedException`. Override it. |
| `virtual Load<T>(string file)` | Reads the whole file as UTF-8 and passes the string to `Deserialize<T>`. Throws `ArgumentNullException` when `file` is null or whitespace or has no file name part. Throws `LocalFileNotFoundException` when the file does not exist. |
| `virtual Save<T>(string file, T deserialized)` | Throws `ArgumentNullException` when `file` is null or whitespace. Deletes an existing file, then calls `Serialize<T>` and copies the stream into a new file. |

### `NewtonsoftJsonSerializerAdapter`

`public sealed class NewtonsoftJsonSerializerAdapter : BaseSerializerAdapter` — JSON adapter based on Newtonsoft.Json. The factory uses it for all JSON.

| Member | Description |
|---|---|
| `NewtonsoftJsonSerializerAdapter()` | Constructor. Uses `JsonSerializerSettings` with `NotNullObserverConverter` (Newtonsoft), culture `pt-BR`, `MissingMemberHandling.Error` and `NullValueHandling.Ignore`. |
| `NewtonsoftJsonSerializerAdapter(JsonSerializerSettings settings)` | Constructor. Uses your settings for deserialization. |
| `DeserializeFromStream<T>(Stream stream, Encoding encoding = null)` | Reads the stream to the end and calls `JsonConvert.DeserializeObject<T>` with the settings. Wraps `NotNullObserverException` in a new `NotNullObserverException(typeof(T), e)`. |
| `Deserialize<T>(object serialized)` | Casts `serialized` to `string` and deserializes it with the settings. Same exception wrapping. |
| `Serialize<T>(T deserialized, out Stream stream)` | Writes indented JSON as UTF-8 into a new `MemoryStream` positioned at 0. This method uses a default `JsonSerializer` and does not apply the configured settings, so `null` properties are written. |

### `JsonSerializerAdapter`

`public sealed class JsonSerializerAdapter : BaseSerializerAdapter` — JSON adapter based on System.Text.Json. The factory never selects it; create it directly.

| Member | Description |
|---|---|
| `JsonSerializerAdapter()` | Constructor. Uses `JsonSerializerOptions` with `WriteIndented = true`, `PropertyNameCaseInsensitive = true`, `DefaultIgnoreCondition = WhenWritingNull` and the System.Text.Json `NotNullObserverConverter`. |
| `JsonSerializerAdapter(JsonSerializerOptions settings)` | Constructor. Uses your options. |
| `DeserializeFromStream<T>(Stream stream, Encoding encoding = null)` | Throws `ArgumentNullException` when `stream` is null. Reads the stream to the end and calls `JsonSerializer.Deserialize<T>`. Wraps `NotNullObserverException` in a new `NotNullObserverException(typeof(T), e)`. |
| `Deserialize<T>(object serialized)` | Casts `serialized` to `string` and deserializes it. Same exception wrapping. |
| `Serialize<T>(T deserialized, out Stream stream)` | Writes JSON with a `Utf8JsonWriter` (indented when `WriteIndented` is set) into a new `MemoryStream` positioned at 0. When `deserialized` is `null`, the stream is empty. |

### `XmlSerializerAdapter`

`public sealed class XmlSerializerAdapter : BaseSerializerAdapter` — XML adapter based on `System.Xml.Serialization.XmlSerializer`.

| Member | Description |
|---|---|
| `DeserializeFromStream<T>(Stream stream, Encoding encoding = null)` | Deserializes with `new XmlSerializer(typeof(T))`. Returns `null` if the result is not a `T`. |
| `Deserialize<T>(object serialized)` | Accepts an `XmlDocument` (uses `OuterXml`) or a `string`. |
| `Serialize<T>(T deserialized, out Stream stream)` | Serializes with `new XmlSerializer(deserialized.GetType())` into a `MemoryStream` positioned at 0. Output is indented UTF-8, keeps the XML declaration, and has no `xsi`/`xsd` namespace declarations. |

### `YamlSerializerAdapter`

`public sealed class YamlSerializerAdapter : BaseSerializerAdapter, IStringSerializerAdapter` — YAML adapter based on [YamlDotNet](https://github.com/aaubry/YamlDotNet). Property names use camel case (`ApplicationName` becomes `applicationName`).

| Member | Description |
|---|---|
| `DeserializeFromStream<T>(Stream stream, Encoding encoding = null)` | Throws `ArgumentNullException` when `stream` is null. Reads the stream (UTF-8 when `encoding` is `null`, byte order marks detected) and deserializes it. The stream is left open. |
| `Deserialize<T>(object serialized)` | Throws `ArgumentNullException` when `serialized` is null and `ArgumentException` when it is not a `string`. Duplicate keys throw `YamlDotNet.Core.YamlException`. |
| `Serialize<T>(T deserialized, out Stream stream)` | Writes YAML as UTF-8 into a new `MemoryStream` positioned at 0. When `deserialized` is `null`, the stream is empty. |
| `SerializeToString<T>(T deserialized)` | Returns the YAML string, or an empty string when `deserialized` is `null`. |

### `SerializerAttribute`

`public sealed class SerializerAttribute : Attribute` — selects the format the factory uses for a class or interface.

Declared with `AttributeTargets.Class | AttributeTargets.Interface`, `Inherited = false`, `AllowMultiple = true`. The factory reads it with `Attribute.GetCustomAttribute`, which throws `AmbiguousMatchException` if the attribute is applied more than once, so apply it only once per type.

| Member | Description |
|---|---|
| `SerializerAttribute(SerializerFormat format = SerializerFormat.Xml, bool isStrict = true)` | Constructor. Default is strict XML. |
| `Format` | The `SerializerFormat`. |
| `IsStrict` | For JSON: `true` fails on unknown JSON members during deserialization, `false` ignores them. Has no effect on XML or YAML. |

### `SerializerFormat`

`public enum SerializerFormat` — output format. Each value has a `[HumanReadable]` label.

| Value | Description |
|---|---|
| `None` (0) | No format. The factory throws `InvalidOperationException` for it. Label `None`. |
| `Json` | JSON through Newtonsoft.Json. Label `JSON`. |
| `Xml` | XML through `XmlSerializer`. Label `XML`. |
| `Yaml` | YAML through YamlDotNet. Label `YAML`. |

### `NotNullObserver`

`public class NotNullObserver` — empty marker type for properties that should never receive a value. See [Not-null observer](#not-null-observer).

### `CrispyWaffle.Serialization.NewtonsoftJson.NotNullObserverConverter`

`public sealed class NotNullObserverConverter : JsonConverter` (Newtonsoft) — enforces `NotNullObserver` properties during Newtonsoft deserialization.

| Member | Description |
|---|---|
| `CanConvert(Type objectType)` | Returns `true` only for `NotNullObserver`. |
| `ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)` | Returns `null` for `null`, a null/whitespace string, or an empty array or object. Otherwise throws `NotNullObserverException` with the token type, value and JSON path. |
| `WriteJson(JsonWriter writer, object value, JsonSerializer serializer)` | Writes `value` with `serializer.Serialize`. |

### `CrispyWaffle.Serialization.NewtonsoftJson.NotNullObserverException`

`public class NotNullObserverException : Exception` — thrown by the Newtonsoft converter and adapter when a `NotNullObserver` property has a value.

| Member | Description |
|---|---|
| `NotNullObserverException(JsonToken type, object value, string path)` | Message: `Not null observer found a not null value in path {path} of type {type}: {value}`. Thrown by the converter. |
| `NotNullObserverException(Type parentType, Exception innerException)` | Message: `Unable to serialize type {parentType.FullName}. Constraints: Not null observer`. Thrown by `NewtonsoftJsonSerializerAdapter` around the converter's exception. |
| `protected NotNullObserverException(SerializationInfo info, StreamingContext context)` | Serialization constructor. |

### `CrispyWaffle.Serialization.SystemTextJson.NotNullObserverConverter`

`public sealed class NotNullObserverConverter : JsonConverter<NotNullObserver>` (System.Text.Json) — enforces `NotNullObserver` properties during System.Text.Json deserialization.

| Member | Description |
|---|---|
| `Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)` | Returns `null` for `null`, a null/whitespace string, or `{}`. Throws `NotNullObserverException` for a non-empty object. Throws `InvalidOperationException` for other non-null values, which System.Text.Json surfaces as a `JsonException` (see the note under [Not-null observer](#not-null-observer)). |
| `Write(Utf8JsonWriter writer, NotNullObserver value, JsonSerializerOptions options)` | Writes `value` with `JsonSerializer.Serialize`. |

### `CrispyWaffle.Serialization.SystemTextJson.NotNullObserverException`

`public class NotNullObserverException : Exception` — thrown by the System.Text.Json converter and adapter when a `NotNullObserver` property has a value.

| Member | Description |
|---|---|
| `NotNullObserverException(JsonTokenType type, object value)` | Message: `Not null observer found a not null value of type {type}: {value}`. Thrown by the converter. |
| `NotNullObserverException(Type parentType, Exception innerException)` | Message: `Unable to serialize type {parentType.FullName}. Constraints: Not null observer`. Thrown by `JsonSerializerAdapter` around the converter's exception. |
| `protected NotNullObserverException(SerializationInfo info, StreamingContext context)` | Serialization constructor. |

The two exception classes have the same name in different namespaces. Catch the one that matches the adapter you use.

### `LocalFileNotFoundException`

`public class LocalFileNotFoundException : Exception` (namespace `CrispyWaffle.Serialization.Adapters`) — thrown by `BaseSerializerAdapter.Load<T>` when the file does not exist.

| Member | Description |
|---|---|
| `LocalFileNotFoundException(string file, string fullPath)` | Message: `Unable to find the file {file} in the directory {fullPath}`. `fullPath` is the directory of the absolute path. |
| `protected LocalFileNotFoundException(SerializationInfo info, StreamingContext context)` | Serialization constructor. |

## Examples

### XML round trip

`[Serializer]` with no arguments selects XML.

```csharp
using CrispyWaffle.Serialization;

var order = new Order { Code = 42, Description = "Sample" };

var xml = (string)order.GetSerializer();

Order copy = SerializerFactory.GetSerializer<Order>().Deserialize(xml);

[Serializer]
public class Order
{
    public int Code { get; set; }

    public string Description { get; set; }
}
```

### Collections

Arrays and generic collections use the attribute of their element type.

```csharp
using System.Collections.Generic;
using CrispyWaffle.Serialization;

var items = new List<Customer>
{
    new() { Id = 1, Name = "Jane" },
    new() { Id = 2, Name = "John" },
};

var json = (string)items.GetSerializer();

List<Customer> copy = new List<Customer>().GetSerializer().Deserialize(json);

[Serializer(SerializerFormat.Json)]
public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; }
}
```

### Save to and load from a file

```csharp
using CrispyWaffle.Serialization;

var settings = new AppSettings { Endpoint = "https://example.com" };

settings.GetSerializer().Save("settings.json");

AppSettings loaded = SerializerFactory.GetSerializer<AppSettings>().Load("settings.json");

[Serializer(SerializerFormat.Json, false)]
public class AppSettings
{
    public string Endpoint { get; set; }
}
```

### Force a format without an attribute

`GetCustomSerializer` ignores the attribute, so it also works for types without one.

```csharp
using CrispyWaffle.Serialization;
using Newtonsoft.Json.Linq;

var point = new Point { X = 1, Y = 2 };

JToken token = point.GetCustomSerializer(SerializerFormat.Json);

public class Point
{
    public int X { get; set; }

    public int Y { get; set; }
}
```

### YAML round trip

`[Serializer(SerializerFormat.Yaml)]` selects YAML. Property names are written in camel case.

```csharp
using CrispyWaffle.Serialization;

var settings = new AppSettings { ApplicationName = "Crispy Waffle", RetryCount = 3 };

// applicationName: Crispy Waffle
// retryCount: 3
var yaml = (string)settings.GetSerializer();

AppSettings copy = SerializerFactory.GetSerializer<AppSettings>().Deserialize(yaml);

settings.GetSerializer().Save("settings.yaml");
AppSettings loaded = SerializerFactory.GetSerializer<AppSettings>().Load("settings.yaml");

[Serializer(SerializerFormat.Yaml)]
public class AppSettings
{
    public string ApplicationName { get; set; }

    public int RetryCount { get; set; }
}
```

To use YAML for a type whose attribute selects another format, call `GetCustomSerializer(SerializerFormat.Yaml)`.

### Use the System.Text.Json adapter directly

```csharp
using System.IO;
using CrispyWaffle.Serialization.Adapters;

var adapter = new JsonSerializerAdapter();

adapter.Serialize(new Point { X = 1, Y = 2 }, out Stream stream);
var json = new StreamReader(stream).ReadToEnd();

Point copy = adapter.Deserialize<Point>(json);

public class Point
{
    public int X { get; set; }

    public int Y { get; set; }
}
```

### Detect an unexpected value

`LegacyField` is expected to always be empty. When it is not, deserialization fails.

```csharp
using System;
using CrispyWaffle.Serialization;
using CrispyWaffle.Serialization.Adapters;
using CrispyWaffle.Serialization.NewtonsoftJson;

var adapter = new NewtonsoftJsonSerializerAdapter();

try
{
    adapter.Deserialize<Payload>("{\"Id\":1,\"LegacyField\":\"surprise\"}");
}
catch (NotNullObserverException e)
{
    // "Unable to serialize type Payload. Constraints: Not null observer"
    Console.WriteLine(e.Message);
    Console.WriteLine(e.InnerException?.Message);
}

public class Payload
{
    public int Id { get; set; }

    public NotNullObserver LegacyField { get; set; }
}
```

## See also

- [Service locator](service-locator.md)
- [Attributes](attributes.md)
- [Logging](logging.md)

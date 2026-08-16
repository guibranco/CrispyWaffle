# Serialization

Crispy Waffle exposes serialization through a common API based on `SerializerFactory` and `SerializerConverter<T>`. JSON and XML remain supported, and the library also provides YAML support through [YamlDotNet](https://github.com/aaubry/YamlDotNet) and TOML support through [Tomlyn](https://github.com/xoofx/Tomlyn).

YAML and TOML use camel-case property names by default and support string, stream, and file-based workflows.

## Choosing a serializer

There are two ways to select a serialization format.

### Use `SerializerAttribute` as the default format

Annotate a class when it should always use a specific serializer:

```csharp
using CrispyWaffle.Serialization;

[Serializer(SerializerFormat.Toml)]
public class AppSettings
{
    public string ApplicationName { get; set; }

    public int RetryCount { get; set; }
}
```

An annotated object can then use `GetSerializer()`:

```csharp
var settings = new AppSettings
{
    ApplicationName = "Crispy Waffle",
    RetryCount = 3,
};

var serializer = settings.GetSerializer();
```

`GetSerializer()` requires the target type, or the element type of a supported collection, to define `SerializerAttribute`.

### Select a format explicitly

Use `GetCustomSerializer()` when the serialization format should be selected at the call site instead of being defined by an attribute:

```csharp
var serializer = settings.GetCustomSerializer(SerializerFormat.Toml);
```

The generic overload can be used when deserializing without an existing object:

```csharp
var serializer = SerializerFactory.GetCustomSerializer<AppSettings>(SerializerFormat.Toml);
```

## YAML

Use `SerializerFormat.Yaml` to serialize with YamlDotNet.

```csharp
var yamlSerializer = settings.GetCustomSerializer(SerializerFormat.Yaml);
var yaml = (string)yamlSerializer;
```

The generated YAML uses camel-case property names:

```yaml
applicationName: Crispy Waffle
retryCount: 3
```

Deserialize the YAML string back to the target type:

```csharp
var loaded = SerializerFactory
    .GetCustomSerializer<AppSettings>(SerializerFormat.Yaml)
    .Deserialize(yaml);
```

A type can also declare YAML as its default format:

```csharp
[Serializer(SerializerFormat.Yaml)]
public class AppSettings
{
    public string ApplicationName { get; set; }

    public int RetryCount { get; set; }
}
```

## TOML

Use `SerializerFormat.Toml` to serialize with Tomlyn.

```csharp
var tomlSerializer = settings.GetCustomSerializer(SerializerFormat.Toml);
var toml = (string)tomlSerializer;
```

The generated TOML uses camel-case property names:

```toml
applicationName = "Crispy Waffle"
retryCount = 3
```

Deserialize the TOML string back to the target type:

```csharp
var loaded = SerializerFactory
    .GetCustomSerializer<AppSettings>(SerializerFormat.Toml)
    .Deserialize(toml);
```

A type can also declare TOML as its default format:

```csharp
[Serializer(SerializerFormat.Toml)]
public class AppSettings
{
    public string ApplicationName { get; set; }

    public int RetryCount { get; set; }
}
```

## Stream serialization

YAML and TOML integrate with the existing stream API. `Serialize(out Stream)` returns a UTF-8 stream positioned at the beginning of the serialized content. The returned stream belongs to the caller and should be disposed after use.

```csharp
var serializer = settings.GetCustomSerializer(SerializerFormat.Toml);
serializer.Serialize(out var stream);

using (stream)
{
    var loaded = SerializerFactory
        .GetCustomSerializer<AppSettings>(SerializerFormat.Toml)
        .DeserializeFromStream(stream);
}
```

`DeserializeFromStream()` leaves the supplied stream open, so the caller remains responsible for its lifetime.

An optional `Encoding` can be supplied when reading a stream. UTF-8 is used when no encoding is specified.

## Saving and loading files

The standard `Save()` and `Load()` APIs work with YAML and TOML:

```csharp
var tomlSerializer = settings.GetCustomSerializer(SerializerFormat.Toml);
tomlSerializer.Save("settings.toml");

var loadedToml = SerializerFactory
    .GetCustomSerializer<AppSettings>(SerializerFormat.Toml)
    .Load("settings.toml");
```

```csharp
var yamlSerializer = settings.GetCustomSerializer(SerializerFormat.Yaml);
yamlSerializer.Save("settings.yaml");

var loadedYaml = SerializerFactory
    .GetCustomSerializer<AppSettings>(SerializerFormat.Yaml)
    .Load("settings.yaml");
```

## String conversion behavior

YAML and TOML implement the string-capable serializer abstraction used by the explicit `string` conversion on `SerializerConverter<T>`:

```csharp
var toml = (string)settings.GetCustomSerializer(SerializerFormat.Toml);
var yaml = (string)settings.GetCustomSerializer(SerializerFormat.Yaml);
```

When the YAML or TOML string serializer receives a `null` object, it returns `string.Empty`. Passing a non-string value to `Deserialize(object)` for these formats throws `ArgumentException`; passing `null` throws `ArgumentNullException`.

Using the Crispy Waffle serialization abstractions keeps application code independent of the underlying YAML and TOML libraries while preserving the same serializer workflow across supported formats.

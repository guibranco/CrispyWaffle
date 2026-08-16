# Serialization

Crispy Waffle provides serializer adapters for JSON, XML, YAML, and TOML. YAML support is implemented with [YamlDotNet](https://github.com/aaubry/YamlDotNet), while TOML support is implemented with [Tomlyn](https://github.com/xoofx/Tomlyn). Both adapters use camel-case property names by default.

## YAML with `SerializerAttribute`

Annotate a class with `SerializerFormat.Yaml` to make YAML its default serializer:

```csharp
using CrispyWaffle.Serialization;

[Serializer(SerializerFormat.Yaml)]
public class AppSettings
{
    public string ApplicationName { get; set; }

    public int RetryCount { get; set; }
}
```

Serialize an instance to a YAML string:

```csharp
var settings = new AppSettings
{
    ApplicationName = "Crispy Waffle",
    RetryCount = 3,
};

var yaml = (string)settings.GetSerializer();
```

The generated YAML uses camel-case property names:

```yaml
applicationName: Crispy Waffle
retryCount: 3
```

Deserialize the YAML back to the annotated type:

```csharp
var settings = SerializerFactory
    .GetSerializer<AppSettings>()
    .Deserialize(yaml);
```

## Saving and loading YAML files

The YAML adapter supports the same file APIs exposed by the other serializer adapters:

```csharp
var serializer = settings.GetSerializer();
serializer.Save("settings.yaml");

var loaded = SerializerFactory
    .GetSerializer<AppSettings>()
    .Load("settings.yaml");
```

## Selecting YAML explicitly

When a type should not use YAML as its default serializer, request it explicitly with `GetCustomSerializer`:

```csharp
var yamlSerializer = settings.GetCustomSerializer(SerializerFormat.Yaml);
var yaml = (string)yamlSerializer;
```

## TOML with `SerializerAttribute`

Annotate a class with `SerializerFormat.Toml` to make TOML its default serializer:

```csharp
using CrispyWaffle.Serialization;

[Serializer(SerializerFormat.Toml)]
public class AppSettings
{
    public string ApplicationName { get; set; }

    public int RetryCount { get; set; }
}
```

Serialize an instance to a TOML string:

```csharp
var settings = new AppSettings
{
    ApplicationName = "Crispy Waffle",
    RetryCount = 3,
};

var toml = (string)settings.GetSerializer();
```

The generated TOML uses camel-case property names:

```toml
applicationName = "Crispy Waffle"
retryCount = 3
```

Deserialize the TOML back to the annotated type:

```csharp
var settings = SerializerFactory
    .GetSerializer<AppSettings>()
    .Deserialize(toml);
```

## Saving and loading TOML files

The TOML adapter supports stream, file, and string serialization through the existing serializer abstraction:

```csharp
var serializer = settings.GetSerializer();
serializer.Save("settings.toml");

var loaded = SerializerFactory
    .GetSerializer<AppSettings>()
    .Load("settings.toml");
```

## Selecting TOML explicitly

When a type should not use TOML as its default serializer, request it explicitly with `GetCustomSerializer`:

```csharp
var tomlSerializer = settings.GetCustomSerializer(SerializerFormat.Toml);
var toml = (string)tomlSerializer;
```

This keeps YAML and TOML consistent with the existing serializer abstraction instead of requiring direct library usage in application code.

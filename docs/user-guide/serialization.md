# Serialization

Crispy Waffle provides serializer adapters for JSON, XML, and YAML. YAML support is implemented with [YamlDotNet](https://github.com/aaubry/YamlDotNet) and uses camel-case property names by default.

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

This keeps YAML consistent with the existing serializer abstraction instead of requiring direct YamlDotNet usage in application code.

# YAML Serialization

The `YamlSerializer` class provides methods for serializing objects to YAML strings and deserializing YAML content back into objects.

## Features

- **Serialize**: Convert objects to YAML format.
- **Deserialize**: Convert YAML content back into objects.

## Example

```csharp
var serializer = new YamlSerializer();
var yaml = serializer.Serialize(new { Name = "Test", Value = 123 });
var obj = serializer.Deserialize<dynamic>(yaml);
```

This feature is built using the YamlDotNet library, ensuring robust and efficient YAML processing.

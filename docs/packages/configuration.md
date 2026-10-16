# CrispyWaffle.Configuration

CrispyWaffle.Configuration holds the connection settings that the connector packages (Redis, CouchDB, Elasticsearch, RabbitMQ, SMTP) take. A `Connection` stores a host, a port and a set of credentials. When it is serialized, the password is encrypted, so you can keep connections in configuration files without writing passwords in plain text.

Use this package whenever you create a connector from one of the other CrispyWaffle packages. You can also use it for your own connection settings.

| | |
|---|---|
| **Package** | `CrispyWaffle.Configuration` |
| **Namespace** | `CrispyWaffle.Configuration` |
| **Install** | `dotnet add package CrispyWaffle.Configuration` |
| **Depends on** | `CrispyWaffle` |

## How it works

`Connection` implements `IConnection`, and its `Credentials` property holds an `IConnectionCredential`. The default constructor sets `Credentials` to a new `Credentials` instance, so you can use object-initializer syntax such as `Credentials = { Username = "...", Password = "..." }` without creating the credentials object yourself.

Connectors read `Host`, `Port`, `Credentials.Username` and `Credentials.Password` from the connection in their constructors. They don't keep the connection after that, so changing a `Connection` after a connector has been created has no effect on that connector.

### Password encryption

`Credentials` has two password properties:

- `Password` holds the plain-text value. It has `[XmlIgnore]` and both `JsonIgnore` attributes (Newtonsoft and System.Text.Json), so serializers never write it.
- `PasswordInternal` is the value that gets serialized, under the name `Password` (XML element, Newtonsoft `JsonProperty` and System.Text.Json `JsonPropertyName`).

When `PasswordInternal` is read, the plain-text password is encrypted with `Security.Encrypt` and a 32-character MD5 hash of the encrypted text is appended. When the password is empty or whitespace, the getter returns an empty string.

When `PasswordInternal` is assigned, the setter does the reverse:

- If the last 32 characters match the MD5 hash of the rest of the value, the rest is decrypted and stored as the plain-text password.
- If they don't match, or the value is 32 characters or shorter, or decryption throws, the value is stored as the plain-text password.

Because of this fallback, a configuration file can hold either an encrypted password or a plain-text one.

The encryption keys come from an `ISecureCredentialProvider` that `Credentials` resolves through `ServiceLocator.Resolve<ISecureCredentialProvider>()` every time `PasswordInternal` is read or assigned. Register one before you serialize or deserialize credentials that have a password. The same keys must be used on both sides.

`Connection` has the `[Serializer]` attribute, which uses XML as the default format. The `Name` property is written as an XML attribute, and the credentials are written through the hidden `CredentialsInternal` property as a `Credentials` element.

## Getting started

```csharp
using CrispyWaffle.Composition;
using CrispyWaffle.Configuration;

// Register the keys used to encrypt and decrypt serialized passwords.
ServiceLocator.Register<ISecureCredentialProvider>(
    () =>
        new SecureCredentialProvider
        {
            PasswordHash = "Cr1$PTVV@FE13",
            SaltKey = "y48H85nH21",
            IVKey = "HZEM7|Ne2YGS/F41",
        },
    Lifetime.Singleton
);

// Describe a connection. Credentials is created by the constructor.
var connection = new Connection
{
    Name = "Redis",
    Host = "localhost",
    Port = 6379,
    Credentials = { Username = "default", Password = "myP@ssw0rd" },
};
```

Pass the connection to a connector constructor, for example `new RedisConnector(connection, serializer)`. See [CrispyWaffle.Redis](redis.md), [CrispyWaffle.CouchDB](couchdb.md) and [CrispyWaffle.ElasticSearch](elasticsearch.md).

## Reference

### `Connection`

`public sealed class Connection : IConnection` is the default implementation of `IConnection`, with XML serialization support.

| Member | Description |
|---|---|
| `Connection()` | Constructor. Sets `Credentials` to a new `Credentials` instance. |
| `Name` | Connection name or identifier. Serialized as the XML attribute `Name`. |
| `Host` | Host name or address. Some connectors expect the URL scheme here (see the CouchDB page). |
| `Port` | Port number. |
| `Credentials` | The `IConnectionCredential` used to authenticate. Has `[XmlIgnore]`. |
| `CredentialsInternal` | Infrastructure property, hidden from IntelliSense. Casts `Credentials` to `Credentials` so the XML serializer can write it as the `Credentials` element. Reading it throws `InvalidCastException` if `Credentials` holds a different `IConnectionCredential` implementation. Don't use it directly. |

### `IConnection`

`public interface IConnection` is the contract that every connector constructor accepts.

| Member | Description |
|---|---|
| `Name` | Connection name or identifier. |
| `Host` | Host name or address. |
| `Port` | Port number. |
| `Credentials` | Credentials used to authenticate. |

### `IConnectionCredential`

`public interface IConnectionCredential` is a username and password pair.

| Member | Description |
|---|---|
| `Username` | User name. |
| `Password` | Plain-text password. |

### `ISecureCredentialProvider`

`public interface ISecureCredentialProvider` supplies the keys that `Credentials` uses to encrypt and decrypt serialized passwords. `Credentials` resolves it from `ServiceLocator`.

| Member | Description |
|---|---|
| `PasswordHash` | Password used to derive the encryption key. |
| `SaltKey` | Salt used to derive the encryption key. |
| `IVKey` | Initialization vector. |

### `Credentials`

`public sealed class Credentials : IConnectionCredential, INotifyPropertyChanged` is the default credential implementation. It encrypts the password when serialized.

| Member | Description |
|---|---|
| `Username` | User name. Raises `PropertyChanged` when set. |
| `Password` | Plain-text password. Never serialized. Raises `PropertyChanged` when set. |
| `PasswordInternal` | Infrastructure property, hidden from IntelliSense, serialized as `Password`. The getter returns the encrypted password followed by its MD5 hash, or an empty string when there is no password. The setter decrypts a value in that format, or stores any other value as plain text. Both resolve `ISecureCredentialProvider` from `ServiceLocator` when there is a value to process. Raises `PropertyChanged` with the name `PasswordInternal`. |
| `PropertyChanged` | Event raised when `Username`, `Password` or `PasswordInternal` is set. |

### `SecureCredentialProvider`

`public class SecureCredentialProvider : ISecureCredentialProvider` is a plain implementation of `ISecureCredentialProvider` with settable properties.

| Member | Description |
|---|---|
| `PasswordHash` | Password used to derive the encryption key. |
| `SaltKey` | Salt used to derive the encryption key. |
| `IVKey` | Initialization vector. |

### `ConnectionNameAttribute`

`public sealed class ConnectionNameAttribute : Attribute` is a marker that records which named connection a connector class expects. You can apply it to a class more than once.

The connectors in the CrispyWaffle packages use it to declare their expected names: `"Redis"`, `"RedisMaster"` and `"RedisSlave"`, `"CouchDB"`, `"ElasticSearch"`, `"RabbitMQ"` and `"SMTP"`. No code in the CrispyWaffle packages reads the attribute. It's metadata for your own configuration-loading code.

| Member | Description |
|---|---|
| `ConnectionNameAttribute(string name)` | Constructor. Sets `Name`. |
| `Name` | The connection name. |
| `Order` | Ordering hint for when a class needs more than one connection, such as `MasterSlaveConfiguration`. Defaults to `0`. |

## Examples

### Round-trip credentials through JSON

The serialized output contains the encrypted password under `Password`. Assigning that value to `PasswordInternal` restores the plain-text password.

```csharp
using CrispyWaffle.Configuration;
using CrispyWaffle.Serialization;
using CrispyWaffle.Utilities;

// Requires an ISecureCredentialProvider registered in ServiceLocator.
var credential = new Credentials { Password = "EchoFoxPapa" };

var json = (string)credential.GetCustomSerializer(SerializerFormat.Json);

dynamic deserialized = SerializerFactory
    .GetCustomSerializer<DynamicSerialization>(SerializerFormat.Json)
    .Deserialize(json);

var restored = new Credentials { PasswordInternal = (string)deserialized.Password };
// restored.Password == "EchoFoxPapa"
```

### Mark a custom connector with its connection name

```csharp
using CrispyWaffle.Configuration;

[ConnectionName("Billing")]
public sealed class BillingApiConnector
{
    public BillingApiConnector(IConnection connection)
    {
        BaseUrl = $"https://{connection.Host}:{connection.Port}";
        ApiUser = connection.Credentials.Username;
    }

    public string BaseUrl { get; }

    public string ApiUser { get; }
}
```

## See also

- [CrispyWaffle.Redis](redis.md)
- [CrispyWaffle.CouchDB](couchdb.md)
- [CrispyWaffle.ElasticSearch](elasticsearch.md)
- [CrispyWaffle.RabbitMQ](rabbitmq.md)
- [Cryptography](../core/cryptography.md)
- [Serialization](../core/serialization.md)
- [Service locator](../core/service-locator.md)

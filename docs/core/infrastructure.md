# Infrastructure and constants

`EnvironmentHelper` exposes facts about the running process: application name and version, host name, user, IP addresses, operating system, and process ID. Log providers and connectors use these values to label what they write, so setting the application name and the current operation early in startup makes logs easier to filter.

The root `Constants` class holds values used by the validation helpers. It is `internal` and is not part of the public API.

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.Infrastructure`, `CrispyWaffle` (`Constants`, internal) |

## How it works

All values are computed once, in the static constructor, the first time any `EnvironmentHelper` member is accessed. `UserAgent` is the only value rebuilt on every access.

| Value | Source |
|---|---|
| `ApplicationName` | Name of the entry assembly (or the calling assembly when there is no entry assembly), with spaces replaced by `-` and converted to lower case. Can be replaced with `SetApplicationName`. |
| `DisplayApplicationName` | `null` until `SetDisplayApplicationName` is called. |
| `Operation` | `null` until `SetOperation` is called. |
| `Host` | `Environment.MachineName`. |
| `UserName` | `Environment.UserName`. |
| `IpAddress` | The first IPv4, non-loopback unicast address of a network interface that is up. Empty string when none is found. |
| `IpAddressExternal` | The public IPv4 address returned by `https://api.ipify.org`. Empty string when the request fails or the response is not an IP address. |
| `ExecutionPath` | `Assembly.Location` of the same assembly used for `ApplicationName`. |
| `Version` | `ProductVersion` of that assembly file, or an empty string when `ExecutionPath` is empty. |
| `VersionDate` | Last write time of that assembly file, local time, formatted as `dd/MM/yyyy HH:mm:ss`. |
| `OperationalSystemVersion` | `Environment.OSVersion`, a hyphen, and `x64` or `x86` depending on whether the operating system is 64-bit. |
| `ProcessId` | ID of the current process. |

Keep these points in mind:

- The first access makes a blocking HTTP request to `api.ipify.org` to get `IpAddressExternal`. On machines without internet access, the first use of `EnvironmentHelper` (often the first log entry) can be slow until that request fails.
- When the entry assembly has no file location (for example, a single-file published application), `ExecutionPath` is empty and building `VersionDate` throws, so the static constructor fails with a `TypeInitializationException`.
- The `Set*` methods only store the value. `SetApplicationName` does not apply the lower-case and `-` normalization used for the default name.

Values are read by the logging and messaging integrations, among others:

- `RollingTextFileLogAdapter` and the Elasticsearch and RabbitMQ log providers include the application name, host, IP addresses, operation, process ID and user agent in each entry. The Redis log provider includes the host and IP addresses.
- The Elasticsearch and RabbitMQ connectors use `ApplicationName` (and `Version` for Elasticsearch) in their default index and exchange names.
- The Redis propagation strategies publish to channels named after `Operation` or `ProcessId`.

## Getting started

```csharp
using System;
using CrispyWaffle.Infrastructure;

// Set names before the first log entry is written.
EnvironmentHelper.SetApplicationName("orders-api");
EnvironmentHelper.SetDisplayApplicationName("Orders API");
EnvironmentHelper.SetOperation("nightly-import");

Console.WriteLine(EnvironmentHelper.ApplicationName); // orders-api
Console.WriteLine(EnvironmentHelper.Host);
Console.WriteLine(EnvironmentHelper.UserAgent); // orders-api/1.0.0 (H:MY-HOST|P:1234|T:1)
```

## Reference

### `EnvironmentHelper`

`public static class EnvironmentHelper` — information about the current application, process and machine.

| Member | Description |
|---|---|
| `SetApplicationName(string applicationName)` | Replaces `ApplicationName`. |
| `SetDisplayApplicationName(string displayApplicationName)` | Sets `DisplayApplicationName`. |
| `SetOperation(string operation)` | Sets `Operation`, a free-form name for the job or operation the process is running. |
| `ApplicationName` | Application name used in logs and default resource names. See the table above for the default. |
| `DisplayApplicationName` | Friendly application name. `null` until set. |
| `Operation` | Current operation name. `null` until set. |
| `Host` | Machine name. |
| `UserName` | Name of the user running the process. |
| `IpAddress` | Local IPv4 address, or an empty string. |
| `IpAddressExternal` | Public IPv4 address, or an empty string. |
| `ExecutionPath` | Full path of the entry assembly file. |
| `Version` | Product version of the entry assembly, or an empty string. |
| `VersionDate` | Last write time of the entry assembly, as `dd/MM/yyyy HH:mm:ss`. |
| `OperationalSystemVersion` | Operating system description and bitness, for example `Microsoft Windows NT 10.0.22631.0 - x64`. |
| `ProcessId` | `int` ID of the current process. |
| `UserAgent` | `{ApplicationName}/{Version} (H:{Host}\|P:{ProcessId}\|T:{ManagedThreadId})`, where the thread ID is that of the thread reading the property. |

### `Constants`

`internal static class Constants` — constants for Brazilian tax document validation. Not accessible from application code; listed here for completeness.

| Member | Value | Description |
|---|---|---|
| `_BrazilianCorporateDocumentName` | `"CNPJ"` | Name of the Brazilian corporate taxpayer document. |
| `_BrazilianCorporateDocumentFullLength` | `14` | Number of digits in a CNPJ. |
| `_BrazilianPersonDocumentName` | `"CPF"` | Name of the Brazilian individual taxpayer document. |
| `_BrazilianPersonDocumentFullLength` | `11` | Number of digits in a CPF. |

These values are used by `PersonalDataValidation` in `CrispyWaffle.Validations` (see [Validations](validations.md)).

## Examples

### Tag log output with the current operation

Set the operation at the start of each job so that log entries and the Redis operation channel carry its name.

```csharp
using CrispyWaffle.Infrastructure;
using CrispyWaffle.Log;

EnvironmentHelper.SetOperation("invoice-export");
LogConsumer.Info("Starting {0} on {1}", EnvironmentHelper.Operation, EnvironmentHelper.Host);
```

### Send a user agent with outgoing HTTP requests

```csharp
using System.Net.Http;
using CrispyWaffle.Infrastructure;

using var client = new HttpClient();
client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", EnvironmentHelper.UserAgent);
```

## See also

- [Logging](logging.md)
- [Telemetry](telemetry.md)
- [Validations](validations.md)
- [Redis](../packages/redis.md)

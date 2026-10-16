# CrispyWaffle.HttpClient

`CrispyWaffle.HttpClient` is a small wrapper around `IHttpClientFactory` that sends JSON requests, retries transient failures with exponential backoff, and returns every result as an `HttpResponse<T>` instead of throwing (unless you ask it to throw). Use it when you want typed JSON calls with retries and a uniform result object, without adding Polly.

The package is standalone: it does not reference the core `CrispyWaffle` package and does not use `ServiceLocator`. It is wired up through `Microsoft.Extensions.DependencyInjection` and consumed by injecting `IHttpClientWrapper`.

| | |
|---|---|
| **Package** | `CrispyWaffle.HttpClient` |
| **Namespace** | `CrispyWaffle.HttpClient`, `CrispyWaffle.HttpClient.Serialization`, `CrispyWaffle.HttpClient.Exceptions` |
| **Install** | Not yet published to NuGet (it is not in the release workflow's package list). Reference the project: `dotnet add reference Src/CrispyWaffle.HttpClient/CrispyWaffle.HttpClient.csproj` |
| **Target framework** | `net10.0` only |
| **Depends on** | No CrispyWaffle packages. `Microsoft.Extensions.Http` (which brings `Microsoft.Extensions.Options`, `Microsoft.Extensions.Logging.Abstractions` and DI abstractions) |

## How it works

### Registration

`AddCrispyWaffleHttpClient` calls `services.AddHttpClient()`, binds `HttpRequestOptions` through `services.Configure`, and registers two singletons: `IJsonSerializer` as `SystemTextJsonSerializer` and `IHttpClientWrapper` as `HttpClientWrapper`. The wrapper also takes an optional `ILogger<HttpClientWrapper>`; if logging is registered, it logs header, read and deserialization problems.

### Request flow

For each call, the wrapper:

1. Merges the registered default options with the per-request `HttpRequestOptions`, if any (see [Per-request options](#per-request-options)).
2. Picks the serializer: `options.Serializer` if set, otherwise the `IJsonSerializer` from DI.
3. Creates a client from `IHttpClientFactory` (`CreateClient(NamedHttpClient)` when `NamedHttpClient` is set, otherwise `CreateClient()`), then:
    - sets `BaseAddress` from the options only if the client has no base address yet;
    - sets `Timeout` to `max(1, TimeoutSeconds)` seconds, overwriting any timeout configured on a named client;
    - adds each `DefaultRequestHeaders` entry to the client's default headers (invalid headers are skipped and logged as warnings);
    - adds `Accept: application/json` if the client has no `Accept` header.
4. Builds a new `HttpRequestMessage` for every attempt. If a body is present, it is serialized with the chosen serializer and sent as UTF-8 `StringContent` with content type `application/json` (or `text/plain` when `UseJsonContentType` is `false`). The headers are also copied onto the message with `TryAddWithoutValidation`.
5. Sends with `HttpCompletionOption.ResponseHeadersRead` through the retry policy below.
6. Reads the body as a string, collects response and content headers into `Headers`, and builds the result:
    - **2xx**: deserializes the body into `T` (an empty or whitespace body gives `default(T)`). A deserialization error gives a failed result with the error `"DeserializationFailed: <message>"`.
    - **Any other status**: a failed result whose `Errors` holds `"HTTP <code> <reason>"` and, if non-empty, the raw body.
    - **No response after retries**: a failed result with `StatusCode = null` and one error (the last exception message, or `"Unknown error"`).

When `ThrowOnError` is `true`, each of the failure cases above throws `HttpClientException` instead of returning a failed result. `Duration` covers all attempts and retry delays, up to the arrival of the final response headers (it does not include reading the body).

### Retry policy

Retries are handled by an internal policy (no Polly dependency). With the default `RetryCount = 3`, a call makes up to 4 attempts.

What is retried:

| Trigger | Retried |
|---|---|
| Status code `>= 500` | Yes |
| `408 Request Timeout` | Yes |
| `429 Too Many Requests` | Yes, honoring `Retry-After` |
| Any other status (including all other 4xx) | No. Returned immediately |
| `HttpRequestException` | Yes |
| `OperationCanceledException` (including `TaskCanceledException` from `HttpClient.Timeout`) | Yes |
| `TimeoutException` | Yes |
| Any other exception | No. It propagates out of the call unwrapped |

Delay before the next attempt, where `attempt` is 0 for the first try:

```text
capped = min(RetryBaseDelayMs * 2^min(attempt, 10), MaxRetryDelayMs)
delay  = max(0, capped + capped * 0.2 * (random[0,1) - 0.5))      // jitter of +/-10%
```

With the defaults (`RetryBaseDelayMs = 200`, `MaxRetryDelayMs = 10000`) the waits are about 200 ms, 400 ms and 800 ms.

For a `429` response with a `Retry-After` header (delta seconds or an HTTP date, read from response or content headers), the delay becomes `min(Retry-After, MaxRetryDelayMs)` with no jitter.

Edge cases to be aware of:

- If the last attempt still returns a retryable status, the policy waits one more delay and then reports "no response". The result has `StatusCode = null` and no body, so the final `5xx`/`429` status is not visible to the caller.
- If `RetryCount <= 0`, the request is sent once and **any** exception is captured into a failed result (or an `HttpClientException` when `ThrowOnError` is `true`), rather than propagating.
- Cancelling the `CancellationToken` while a retry is pending throws `TaskCanceledException` from the delay. If cancellation hits the final attempt, the call returns a failed "no response" result instead.
- A `Retry-After` date in the past produces a negative delay, and `Task.Delay` throws `ArgumentOutOfRangeException`.

### Per-request options

When you pass an `HttpRequestOptions` to a call, it is merged with the registered defaults like this:

| Property | Merge rule |
|---|---|
| `BaseAddress`, `NamedHttpClient`, `Serializer` | Per-request value if not `null`, else default |
| `TimeoutSeconds`, `RetryCount`, `RetryBaseDelayMs`, `MaxRetryDelayMs` | Per-request value if not `0`, else default |
| `ThrowOnError`, `UseJsonContentType` | Always the per-request value |
| `DefaultRequestHeaders` | Union of both; per-request keys win (case-insensitive) |

Because a new `HttpRequestOptions` already has non-zero numeric defaults (30, 3, 200, 10000) and `ThrowOnError = false`, passing one replaces your registered values for those properties with the class defaults unless you set them again. Setting `RetryCount = 0` per request does not disable retries; it falls back to the registered default. To disable retries, set `RetryCount = 0` in the registered options.

### Naming conflicts

- `HttpRequestOptions` also exists as `System.Net.Http.HttpRequestOptions`. Projects with `ImplicitUsings` enabled import `System.Net.Http` globally, so naming the type alone gives error CS0104 (ambiguous reference). Add an alias: `using HttpRequestOptions = CrispyWaffle.HttpClient.HttpRequestOptions;`. The examples on this page do this.
- Inside a namespace that starts with `CrispyWaffle`, the name `HttpClient` resolves to the `CrispyWaffle.HttpClient` namespace, not `System.Net.Http.HttpClient`. Use the fully qualified name or an alias if you need the BCL type there.

## Getting started

```csharp
using CrispyWaffle.HttpClient;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging();
services.AddCrispyWaffleHttpClient(o =>
{
    o.BaseAddress = "https://api.example.com/";
    o.TimeoutSeconds = 10;
    o.RetryCount = 2;
});

using var provider = services.BuildServiceProvider();
var http = provider.GetRequiredService<IHttpClientWrapper>();

HttpResponse<Todo> response = await http.GetAsync<Todo>("todos/1");
if (response.IsSuccess)
{
    Console.WriteLine($"{response.Data!.Title} ({response.Duration.TotalMilliseconds} ms)");
}
else
{
    Console.WriteLine($"Failed: {response.StatusCode} {string.Join("; ", response.Errors ?? [])}");
}

public sealed record Todo(int Id, string Title, bool Completed);
```

## Reference

### `ServiceCollectionExtensions`

`public static class ServiceCollectionExtensions` — DI registration for the wrapper (namespace `CrispyWaffle.HttpClient`).

| Member | Description |
|---|---|
| `AddCrispyWaffleHttpClient(this IServiceCollection services, Action<HttpRequestOptions>? configureOptions = null)` | Calls `AddHttpClient()`, configures `HttpRequestOptions` with `configureOptions` (or an empty configuration), and registers `IJsonSerializer` -> `SystemTextJsonSerializer` and `IHttpClientWrapper` -> `HttpClientWrapper`, both as singletons. Returns `services`. To use a different serializer, register your own `IJsonSerializer` after this call so it replaces the default. |

### `IHttpClientWrapper`

`public interface IHttpClientWrapper` — the high-level HTTP API. Inject this.

| Member | Description |
|---|---|
| `GetAsync<T>(string url, HttpRequestOptions? options = null, CancellationToken cancellationToken = default)` | Sends `GET` and deserializes a 2xx body into `T`. Returns `Task<HttpResponse<T>>`. |
| `PostAsync<TRequest, TResponse>(string url, TRequest? body = default, HttpRequestOptions? options = null, CancellationToken cancellationToken = default)` | Sends `POST` with `body` serialized as JSON (no content when `body` is `null`). Returns `Task<HttpResponse<TResponse>>`. |
| `PutAsync<TRequest, TResponse>(string url, TRequest? body = default, HttpRequestOptions? options = null, CancellationToken cancellationToken = default)` | Same as `PostAsync` with `PUT`. |
| `DeleteAsync(string url, HttpRequestOptions? options = null, CancellationToken cancellationToken = default)` | Sends `DELETE`. Returns `Task<HttpResponse<object?>>`; a JSON body is deserialized as `object` (a `JsonElement` with the default serializer). |
| `Get<T>(url, options)` / `Post<TRequest, TResponse>(url, body, options)` / `Put<TRequest, TResponse>(url, body, options)` / `Delete(url, options)` | Synchronous versions that block on the async methods with `GetAwaiter().GetResult()`. No cancellation token. They can deadlock under a synchronization context; prefer the async methods. |

`url` is either absolute or relative to `BaseAddress`. A relative URL with no base address throws `InvalidOperationException` from `HttpClient`, which is not retried and propagates (when `RetryCount > 0`).

### `HttpClientWrapper`

`public class HttpClientWrapper : IHttpClientWrapper` — the default implementation described in [How it works](#how-it-works).

| Member | Description |
|---|---|
| `HttpClientWrapper(IHttpClientFactory httpClientFactory, IOptions<HttpRequestOptions> options, IJsonSerializer? serializer = null, ILogger<HttpClientWrapper>? logger = null)` | Constructor. Throws `ArgumentNullException` when `httpClientFactory` is `null`. Uses `new HttpRequestOptions()` when `options` or its value is `null`, and `new SystemTextJsonSerializer()` when `serializer` is `null`. |
| All `IHttpClientWrapper` members | See above. |

### `HttpRequestOptions`

`public class HttpRequestOptions` — registered defaults (through `IOptions<HttpRequestOptions>`) and per-request overrides.

| Member | Description |
|---|---|
| `BaseAddress` | `string?`. Base URL for the created client. Only applied if the client has no base address already. Default `null`. |
| `TimeoutSeconds` | `int`. Per-attempt timeout; values below 1 become 1. Default `30`. |
| `RetryCount` | `int`. Number of retries after the first attempt. `0` or less means a single attempt. Default `3`. |
| `RetryBaseDelayMs` | `int`. Base delay for exponential backoff. Default `200`. |
| `MaxRetryDelayMs` | `int`. Upper bound for one retry delay, including `Retry-After`. Default `10000`. |
| `ThrowOnError` | `bool`. Throw `HttpClientException` on failure instead of returning a failed `HttpResponse<T>`. Default `false`. |
| `DefaultRequestHeaders` | `IDictionary<string, string>?`. Headers added to every request. Default `null`. |
| `NamedHttpClient` | `string?`. Name passed to `IHttpClientFactory.CreateClient(name)`. Default `null` (unnamed client). |
| `UseJsonContentType` | `bool`. Send request bodies as `application/json`; when `false`, `text/plain`. The body is serialized with the JSON serializer either way. Default `true`. |
| `Serializer` | `IJsonSerializer?`. Serializer override. Default `null` (use the one from DI). |

### `HttpResponse<T>`

`public class HttpResponse<T>` — the result of every wrapper call.

| Member | Description |
|---|---|
| `IsSuccess` | `true` for a 2xx response whose body deserialized without error. |
| `StatusCode` | `HttpStatusCode?`. `null` when no response was received. |
| `Data` | `T?`. The deserialized body; `default` on failure or empty body. |
| `RawContent` | `string?`. The response body as text. |
| `Errors` | `IList<string>?`. Failure messages; `null` on success. |
| `Headers` | `IDictionary<string, IEnumerable<string>>?`. Response and content headers. `null` when no response was received. |
| `Duration` | `TimeSpan`. Time spent on all attempts and retry delays, up to the final response headers. |
| `Success(T? data, HttpStatusCode statusCode, string? rawContent, IDictionary<string, IEnumerable<string>>? headers, TimeSpan duration)` | Static factory for a successful result. |
| `Failure(HttpStatusCode? statusCode, string? rawContent, IList<string>? errors, IDictionary<string, IEnumerable<string>>? headers, TimeSpan duration)` | Static factory for a failed result. |

### `IJsonSerializer`

`public interface IJsonSerializer` — pluggable serializer (namespace `CrispyWaffle.HttpClient.Serialization`). Implementations must be thread-safe because the registered instance is a singleton.

| Member | Description |
|---|---|
| `Serialize<T>(T value)` | Returns the JSON string for `value`. Used for request bodies. |
| `Deserialize<T>(string json)` | Returns `json` as `T`. Used for response bodies. |
| `Deserialize(string json, Type type)` | Non-generic form. Not used by the wrapper. |

### `SystemTextJsonSerializer`

`public class SystemTextJsonSerializer : IJsonSerializer` — the default serializer, based on `System.Text.Json`.

| Member | Description |
|---|---|
| `SystemTextJsonSerializer(JsonSerializerOptions? options = null)` | Constructor. When `options` is `null`, uses camelCase property names, case-insensitive property matching, and `DefaultIgnoreCondition = WhenWritingNull`. |
| `Serialize<T>(T value)` | `JsonSerializer.Serialize(value, options)`. |
| `Deserialize<T>(string json)` | Returns `default` for null or whitespace input, otherwise `JsonSerializer.Deserialize<T>`. |
| `Deserialize(string json, Type type)` | Returns `null` for null or whitespace input, otherwise `JsonSerializer.Deserialize(json, type, options)`. |

### `HttpClientException`

`public class HttpClientException : Exception` — thrown on failure when `ThrowOnError` is `true` (namespace `CrispyWaffle.HttpClient.Exceptions`).

| Member | Description |
|---|---|
| `HttpClientException(string message, HttpStatusCode? statusCode = null, string? responseContent = null, Exception? inner = null)` | Constructor. |
| `StatusCode` | `HttpStatusCode?`. Status of the failed response; `null` when no response was received. |
| `ResponseContent` | `string?`. Response body, when available. |

Thrown with these messages:

| Message | Situation | `StatusCode` | `InnerException` |
|---|---|---|---|
| `HTTP request failed after retries` | No usable response after all attempts | `null` | Last exception, if any |
| `Failed to read response content` | Reading the body threw | Response status | The read exception |
| `Failed to deserialize response to <T>` | 2xx body could not be deserialized | Response status | The deserialization exception |
| `HTTP request failed with <code>` | Non-2xx, non-retryable status | Response status | `null` |

## Examples

### Post a body and throw on failure

Set `ThrowOnError` per request to get exceptions instead of a failed result. Remember that a per-request options object also resets the numeric settings to their class defaults.

```csharp
using CrispyWaffle.HttpClient;
using CrispyWaffle.HttpClient.Exceptions;
using HttpRequestOptions = CrispyWaffle.HttpClient.HttpRequestOptions;

public sealed class OrdersClient(IHttpClientWrapper http)
{
    public async Task<OrderCreated?> CreateAsync(NewOrder order, CancellationToken ct)
    {
        try
        {
            var response = await http.PostAsync<NewOrder, OrderCreated>(
                "orders",
                order,
                new HttpRequestOptions { ThrowOnError = true, TimeoutSeconds = 5 },
                ct
            );
            return response.Data;
        }
        catch (HttpClientException ex)
        {
            Console.WriteLine($"Order API failed: {ex.StatusCode} {ex.ResponseContent}");
            return null;
        }
    }
}

public sealed record NewOrder(string Sku, int Quantity);

public sealed record OrderCreated(Guid Id);
```

### Use a named client and per-request headers

Configure the named client with `AddHttpClient`, then point the wrapper at it. The base address from the named client is kept; `TimeoutSeconds` from the options still overrides its timeout.

```csharp
using CrispyWaffle.HttpClient;
using Microsoft.Extensions.DependencyInjection;
using HttpRequestOptions = CrispyWaffle.HttpClient.HttpRequestOptions;

var services = new ServiceCollection();
services.AddHttpClient("github", c =>
{
    c.BaseAddress = new Uri("https://api.github.com/");
    c.DefaultRequestHeaders.UserAgent.ParseAdd("crispy-waffle-sample");
});
services.AddCrispyWaffleHttpClient(o => o.NamedHttpClient = "github");

using var provider = services.BuildServiceProvider();
var http = provider.GetRequiredService<IHttpClientWrapper>();

var response = await http.GetAsync<Repo>(
    "repos/guibranco/CrispyWaffle",
    new HttpRequestOptions
    {
        DefaultRequestHeaders = new Dictionary<string, string>
        {
            ["X-GitHub-Api-Version"] = "2022-11-28",
        },
    }
);
Console.WriteLine(response.Data?.FullName);

public sealed record Repo(string FullName, int StargazersCount);
```

### Use custom JSON settings

Register your own `IJsonSerializer` after `AddCrispyWaffleHttpClient` to replace the default for every call, or set `HttpRequestOptions.Serializer` for a single call.

```csharp
using System.Text.Json;
using CrispyWaffle.HttpClient;
using CrispyWaffle.HttpClient.Serialization;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCrispyWaffleHttpClient();
services.AddSingleton<IJsonSerializer>(
    new SystemTextJsonSerializer(
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }
    )
);
```

## See also

- [Diagrams](../diagrams.md)
- [Serialization](../core/serialization.md) for the core package's serializers
- [Background jobs](background-jobs.md), the other standalone `Microsoft.Extensions` based package

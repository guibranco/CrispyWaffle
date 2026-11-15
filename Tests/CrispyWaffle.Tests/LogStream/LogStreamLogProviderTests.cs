using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrispyWaffle.Configuration;
using CrispyWaffle.Log;
using CrispyWaffle.LogStream;
using CrispyWaffle.LogStream.Log;
using CrispyWaffle.Serialization;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CrispyWaffle.Tests.LogStream;

public class LogStreamLogProviderTests
{
    private static readonly Uri _server = new("https://logs.example.com/");

    [Fact]
    public async Task FlushShouldPostABatchWithBearerAuthentication()
    {
        // Arrange
        using var handler = new RecordingHandler();
        using var provider = CreateProvider(handler, o => o.StaticContext["region"] = "eu-west-1");
        provider.SetLevel(LogLevel.All);

        // Act
        provider.Info("Startup", "Service started");
        provider.Fatal("Startup", "Out of memory");
        await provider.FlushAsync();

        // Assert
        var request = handler.Requests.Should().ContainSingle().Subject;
        request.Method.Should().Be(HttpMethod.Post);
        request.Uri.Should().Be(new Uri("https://logs.example.com/api/logs"));
        request.Headers["Authorization"].Should().Be("Bearer lsa_secret");
        request.Headers.Should().NotContainKey("X-Api-Key");
        request.Headers["User-Agent"].Should().Contain("CrispyWaffle.LogStream/");

        var body = request.Json;
        body["app_key"]!.Value<string>().Should().Be("billing-api");
        body["app_id"]!.Value<string>().Should().Be("production");
        Guid.TryParse(body["batch_id"]!.Value<string>(), out _).Should().BeTrue();

        var logs = (JArray)body["logs"];
        logs.Should().HaveCount(2);
        logs[0]["level"]!.Value<string>().Should().Be("info");
        logs[0]["category"]!.Value<string>().Should().Be("Startup");
        logs[0]["message"]!.Value<string>().Should().Be("Service started");
        logs[0]["timestamp"].Should().NotBeNull();
        logs[0]["context"]!["region"]!.Value<string>().Should().Be("eu-west-1");
        logs[0]["context"]!["process_id"].Should().NotBeNull();
        logs[1]["level"]!.Value<string>().Should().Be("critical");
    }

    [Fact]
    public async Task FlushShouldUseClientKeyAndTokenWhenNoSecretIsSet()
    {
        // Arrange
        using var handler = new RecordingHandler();
        using var provider = CreateProvider(
            handler,
            o =>
            {
                o.ApiSecret = null;
                o.AppKey = null;
                o.ApiKey = "billing-api";
                o.ApiToken = "lss_token";
            }
        );
        provider.SetLevel(LogLevel.All);

        // Act
        provider.Warning("Payments", "Slow response");
        await provider.FlushAsync();

        // Assert
        var request = handler.Requests.Should().ContainSingle().Subject;
        request.Headers["X-Api-Key"].Should().Be("billing-api");
        request.Headers["X-Api-Token"].Should().Be("lss_token");
        request.Headers.Should().NotContainKey("Authorization");
        request.Json["app_key"].Should().BeNull();
        request.Json["logs"]![0]!["level"]!.Value<string>().Should().Be("warning");
    }

    [Fact]
    public async Task LevelsThatAreNotEnabledShouldNotBeSent()
    {
        // Arrange
        using var handler = new RecordingHandler();
        using var provider = CreateProvider(handler);
        provider.SetLevel(LogLevel.Production);

        // Act
        provider.Debug("Cat", "debug");
        provider.Trace("Cat", "trace");
        provider.Error("Cat", "error");
        await provider.FlushAsync();

        // Assert
        var logs = (JArray)handler.Requests.Should().ContainSingle().Subject.Json["logs"];
        logs.Should().ContainSingle();
        logs[0]["level"]!.Value<string>().Should().Be("error");
    }

    [Fact]
    public async Task NoLevelShouldBeEnabledUntilSetLevelIsCalled()
    {
        // Arrange
        using var handler = new RecordingHandler();
        using var provider = CreateProvider(handler);

        // Act
        provider.Fatal("Cat", "ignored");
        await provider.FlushAsync();

        // Assert
        handler.Requests.Should().BeEmpty();
        provider.PendingCount.Should().Be(0);
    }

    [Fact]
    public async Task TraceShouldUseTheConfiguredTraceLevel()
    {
        // Arrange
        using var handler = new RecordingHandler();
        using var provider = CreateProvider(handler, o => o.TraceLevel = LogStreamLevels.Notice);
        provider.SetLevel(LogLevel.All);

        // Act
        provider.Trace("Cat", "detailed");
        provider.Debug("Cat", "debug");
        await provider.FlushAsync();

        // Assert
        var logs = (JArray)handler.Requests.Single().Json["logs"];
        logs[0]["level"]!.Value<string>().Should().Be("notice");
        logs[1]["level"]!.Value<string>().Should().Be("debug");
    }

    [Fact]
    public async Task TraceWithAnExceptionShouldSendOneErrorEntryWithTheExceptionChain()
    {
        // Arrange
        using var handler = new RecordingHandler();
        using var provider = CreateProvider(handler);
        provider.SetLevel(LogLevel.All);
        var exception = Throw(
            new InvalidOperationException("Charge failed", new TimeoutException("Gateway timeout"))
        );

        // Act
        provider.Trace("Payments", "Could not charge invoice", exception);
        provider.Trace("Payments", exception);
        await provider.FlushAsync();

        // Assert
        var logs = (JArray)handler.Requests.Single().Json["logs"];
        logs.Should().HaveCount(2);

        logs[0]["level"]!.Value<string>().Should().Be("error");
        logs[0]["message"]!.Value<string>().Should().Be("Could not charge invoice");
        var details = logs[0]["context"]!["exception"]!;
        details["type"]!.Value<string>().Should().Be(typeof(InvalidOperationException).FullName);
        details["message"]!.Value<string>().Should().Be("Charge failed");
        details["stack_trace"]!.Value<string>().Should().NotBeNullOrWhiteSpace();
        details["inner_exception"]!["type"]!
            .Value<string>()
            .Should()
            .Be(typeof(TimeoutException).FullName);

        logs[1]["message"]!.Value<string>().Should().Be("Charge failed");
    }

    [Fact]
    public async Task DebugContentShouldBeSentAsATemplatedEntry()
    {
        // Arrange
        using var handler = new RecordingHandler();
        using var provider = CreateProvider(handler);
        provider.SetLevel(LogLevel.All);

        // Act
        provider.Debug("Http", "{\"id\":1}", "response.json");
        provider.Debug("Http", new Sample { Name = "waffle" }, "sample", SerializerFormat.Json);
        await provider.FlushAsync();

        // Assert
        var logs = (JArray)handler.Requests.Single().Json["logs"];
        logs[0]["level"]!.Value<string>().Should().Be("debug");
        logs[0]["message"]!.Value<string>().Should().Be("Debug content response.json");
        logs[0]["message_template"]!.Value<string>().Should().Be("Debug content {identifier}");
        logs[0]["context"]!["identifier"]!.Value<string>().Should().Be("response.json");
        logs[0]["context"]!["content"]!.Value<string>().Should().Be("{\"id\":1}");
        logs[1]["context"]!["content"]!.Value<string>().Should().Contain("waffle");
    }

    [Fact]
    public async Task LongCategoriesShouldBeTruncatedAndEmptyOnesDefaulted()
    {
        // Arrange
        using var handler = new RecordingHandler();
        using var provider = CreateProvider(handler);
        provider.SetLevel(LogLevel.All);

        // Act
        provider.Info(new string('c', 150), "long");
        provider.Info(null, "none");
        await provider.FlushAsync();

        // Assert
        var logs = (JArray)handler.Requests.Single().Json["logs"];
        logs[0]["category"]!.Value<string>().Should().HaveLength(100);
        logs[1]["category"]!.Value<string>().Should().Be("general");
    }

    [Fact]
    public async Task FlushShouldSplitTheQueueIntoBatches()
    {
        // Arrange
        using var handler = new RecordingHandler();
        using var provider = CreateProvider(handler, o => o.BatchSize = 2);
        provider.SetLevel(LogLevel.All);

        // Act
        for (var i = 0; i < 5; i++)
        {
            provider.Info("Cat", "message " + i);
        }

        await provider.FlushAsync();

        // Assert
        var messages = handler
            .Requests.SelectMany(r => (JArray)r.Json["logs"])
            .Select(l => l["message"]!.Value<string>())
            .ToList();
        handler.Requests.Should().HaveCount(3);
        messages.Should().Equal("message 0", "message 1", "message 2", "message 3", "message 4");
    }

    [Fact]
    public async Task TheBackgroundSenderShouldSendWithoutAnExplicitFlush()
    {
        // Arrange
        using var handler = new RecordingHandler();
        using var provider = CreateProvider(
            handler,
            o => o.FlushInterval = TimeSpan.FromMilliseconds(50)
        );
        provider.SetLevel(LogLevel.All);

        // Act
        provider.Info("Cat", "background");
        var sent = await handler.WaitForRequestsAsync(1, TimeSpan.FromSeconds(10));

        // Assert
        sent.Should().BeTrue();
    }

    [Fact]
    public async Task TransientFailuresShouldBeRetried()
    {
        // Arrange
        using var handler = new RecordingHandler(
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.Accepted
        );
        var errors = new ConcurrentBag<Exception>();
        using var provider = CreateProvider(handler, o => o.OnError = errors.Add);
        provider.SetLevel(LogLevel.All);

        // Act
        provider.Info("Cat", "eventually delivered");
        await provider.FlushAsync();

        // Assert
        handler.Requests.Should().HaveCount(3);
        handler
            .Requests.Select(r => r.Json["batch_id"]!.Value<string>())
            .Distinct()
            .Should()
            .ContainSingle();
        provider.DroppedCount.Should().Be(0);
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ABatchShouldBeDroppedAfterTheLastRetry()
    {
        // Arrange
        using var handler = new RecordingHandler(HttpStatusCode.BadGateway);
        var errors = new ConcurrentBag<Exception>();
        using var provider = CreateProvider(
            handler,
            o =>
            {
                o.MaxRetries = 2;
                o.OnError = errors.Add;
            }
        );
        provider.SetLevel(LogLevel.All);

        // Act
        provider.Info("Cat", "lost");
        await provider.FlushAsync();

        // Assert
        handler.Requests.Should().HaveCount(3);
        provider.DroppedCount.Should().Be(1);
        var error = errors
            .Should()
            .ContainSingle()
            .Subject.Should()
            .BeOfType<LogStreamException>()
            .Subject;
        error.StatusCode.Should().Be(502);
        provider.PendingCount.Should().Be(0);
    }

    [Fact]
    public async Task ClientErrorsShouldNotBeRetried()
    {
        // Arrange
        using var handler = new RecordingHandler(HttpStatusCode.Unauthorized);
        var errors = new ConcurrentBag<Exception>();
        using var provider = CreateProvider(handler, o => o.OnError = errors.Add);
        provider.SetLevel(LogLevel.All);

        // Act
        provider.Info("Cat", "rejected");
        await provider.FlushAsync();

        // Assert
        handler.Requests.Should().ContainSingle();
        provider.DroppedCount.Should().Be(1);
        errors.OfType<LogStreamException>().Single().StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task EntryErrorsInAnAcceptedResponseShouldBeReported()
    {
        // Arrange
        using var handler = new RecordingHandler
        {
            ResponseBody = "{\"accepted\":1,\"entries\":[],\"errors\":[\"Entry #1: bad level\"]}",
        };
        var errors = new ConcurrentBag<Exception>();
        using var provider = CreateProvider(handler, o => o.OnError = errors.Add);
        provider.SetLevel(LogLevel.All);

        // Act
        provider.Info("Cat", "one");
        provider.Info("Cat", "two");
        await provider.FlushAsync();

        // Assert
        provider.DroppedCount.Should().Be(1);
        errors.Single().Message.Should().Contain("Entry #1: bad level");
    }

    [Fact]
    public async Task EntriesBeyondTheQueueLimitShouldBeDropped()
    {
        // Arrange
        var gate = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var handler = new RecordingHandler { Gate = gate.Task };
        var errors = new ConcurrentBag<Exception>();
        using var provider = CreateProvider(
            handler,
            o =>
            {
                o.BatchSize = 2;
                o.QueueLimit = 2;
                o.OnError = errors.Add;
            }
        );
        provider.SetLevel(LogLevel.All);

        // Act
        for (var i = 0; i < 5; i++)
        {
            provider.Info("Cat", "message " + i);
        }

        gate.SetResult(true);
        await provider.FlushAsync();

        // Assert
        var sent = handler.Requests.SelectMany(r => (JArray)r.Json["logs"]).Count();
        provider.DroppedCount.Should().BeGreaterThan(0);
        (sent + provider.DroppedCount).Should().Be(5);
        errors.Should().NotBeEmpty().And.AllBeOfType<LogStreamException>();
    }

    [Fact]
    public void DisposeShouldSendQueuedEntries()
    {
        // Arrange
        using var handler = new RecordingHandler();
        var provider = CreateProvider(handler, o => o.FlushInterval = TimeSpan.FromHours(1));
        provider.SetLevel(LogLevel.All);
        provider.Info("Cat", "sent on shutdown");

        // Act
        provider.Dispose();

        // Assert
        handler
            .Requests.SelectMany(r => (JArray)r.Json["logs"])
            .Select(l => l["message"]!.Value<string>())
            .Should()
            .Contain("sent on shutdown");
    }

    [Fact]
    public async Task TheCurrentActivityShouldProvideTheTraceId()
    {
        // Arrange
        using var handler = new RecordingHandler();
        using var provider = CreateProvider(handler);
        provider.SetLevel(LogLevel.All);
        using var activity = new Activity("checkout").SetIdFormat(ActivityIdFormat.W3C).Start();

        // Act
        provider.Info("Checkout", "inside activity");
        activity.Stop();
        provider.Info("Checkout", "outside activity");
        await provider.FlushAsync();

        // Assert
        var logs = (JArray)handler.Requests.Single().Json["logs"];
        logs[0]["trace_id"]!.Value<string>().Should().Be(activity.TraceId.ToHexString());
        logs[0]["context"]!["span_id"]!.Value<string>().Should().Be(activity.SpanId.ToHexString());
        logs[1]["trace_id"].Should().BeNull();
    }

    [Fact]
    public async Task EnvironmentDetailsCanBeTurnedOff()
    {
        // Arrange
        using var handler = new RecordingHandler();
        using var provider = CreateProvider(handler, o => o.IncludeEnvironment = false);
        provider.SetLevel(LogLevel.All);

        // Act
        provider.Info("Cat", "bare");
        await provider.FlushAsync();

        // Assert
        handler.Requests.Single().Json["logs"]![0]!["context"]!
            .Children()
            .Should()
            .BeEmpty();
    }

    [Theory]
    [InlineData(null, "production", "lsa", null, null)]
    [InlineData("billing-api", null, "lsa", null, null)]
    [InlineData(null, "production", "lsa", "key", null)]
    [InlineData("billing-api", "production", null, "key", null)]
    public void InvalidOptionsShouldBeRejected(
        string appKey,
        string appId,
        string secret,
        string apiKey,
        string apiToken
    )
    {
        // Arrange
        var options = new LogStreamOptions
        {
            ServerUrl = _server,
            AppKey = appKey,
            AppId = appId,
            ApiSecret = secret,
            ApiKey = apiKey,
            ApiToken = apiToken,
        };

        // Act
        var act = () => new LogStreamConnector(options);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AConnectionWithAUserNameShouldUseClientCredentials()
    {
        // Arrange
        var connection = new Connection
        {
            Host = "logs.example.com",
            Port = 443,
            Credentials = new Credentials { Username = "billing-api", Password = "lss_token" },
        };

        // Act
        var connector = new LogStreamConnector(connection, null, "production");

        // Assert
        connector.IngestUri.Should().Be(new Uri("https://logs.example.com/api/logs"));
        connector.Options.ApiKey.Should().Be("billing-api");
        connector.Options.ApiToken.Should().Be("lss_token");
        connector.Options.ApiSecret.Should().BeNull();
    }

    [Fact]
    public void AConnectionWithOnlyAPasswordShouldUseTheSharedSecret()
    {
        // Arrange
        var connection = new Connection
        {
            Host = "http://localhost:8081/logstream",
            Credentials = new Credentials { Password = "lsa_secret" },
        };

        // Act
        var connector = new LogStreamConnector(connection, "billing-api", "development");

        // Assert
        connector.IngestUri.Should().Be(new Uri("http://localhost:8081/logstream/api/logs"));
        connector.Options.ApiSecret.Should().Be("lsa_secret");
        connector.Options.ApiKey.Should().BeNull();
    }

    [Theory]
    [InlineData(LogLevel.Fatal, "critical")]
    [InlineData(LogLevel.Error, "error")]
    [InlineData(LogLevel.Warning, "warning")]
    [InlineData(LogLevel.Info, "info")]
    [InlineData(LogLevel.Trace, "debug")]
    [InlineData(LogLevel.Debug, "debug")]
    public void LevelsShouldMapToLogStreamLevels(LogLevel level, string expected) =>
        LogStreamLevels.From(level, new LogStreamOptions()).Should().Be(expected);

    private static LogStreamLogProvider CreateProvider(
        RecordingHandler handler,
        Action<LogStreamOptions> configure = null
    )
    {
        var options = new LogStreamOptions
        {
            ServerUrl = _server,
            AppKey = "billing-api",
            AppId = "production",
            ApiSecret = "lsa_secret",
            RetryDelay = TimeSpan.FromMilliseconds(1),
            FlushInterval = TimeSpan.FromHours(1),
            ShutdownTimeout = TimeSpan.FromSeconds(5),
        };
        configure?.Invoke(options);

        return new LogStreamLogProvider(
            new LogStreamConnector(options, new System.Net.Http.HttpClient(handler, false))
        );
    }

    private static Exception Throw(Exception exception)
    {
        try
        {
            throw exception;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public sealed class Sample
    {
        public string Name { get; set; }
    }

    private sealed class RecordedRequest
    {
        public HttpMethod Method { get; init; }

        public Uri Uri { get; init; }

        public Dictionary<string, string> Headers { get; init; }

        public JObject Json { get; init; }
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Queue<HttpStatusCode> _statuses;
        private readonly HttpStatusCode _last;
        private readonly List<RecordedRequest> _requests = new();
        private readonly SemaphoreSlim _received = new(0);

        public RecordingHandler(params HttpStatusCode[] statuses)
        {
            _statuses = new Queue<HttpStatusCode>(statuses);
            _last = statuses.Length > 0 ? statuses[^1] : HttpStatusCode.Accepted;
        }

        public Task Gate { get; init; }

        public string ResponseBody { get; init; } =
            "{\"accepted\":1,\"entries\":[],\"errors\":null}";

        public IReadOnlyList<RecordedRequest> Requests
        {
            get
            {
                lock (_requests)
                {
                    return _requests.ToList();
                }
            }
        }

        public async Task<bool> WaitForRequestsAsync(int count, TimeSpan timeout)
        {
            for (var i = 0; i < count; i++)
            {
                if (!await _received.WaitAsync(timeout))
                {
                    return false;
                }
            }

            return true;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            if (Gate != null)
            {
                await Gate;
            }

            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            var headers = request
                .Headers.Concat(request.Content.Headers)
                .ToDictionary(
                    h => h.Key,
                    h => string.Join(",", h.Value),
                    StringComparer.OrdinalIgnoreCase
                );

            lock (_requests)
            {
                _requests.Add(
                    new RecordedRequest
                    {
                        Method = request.Method,
                        Uri = request.RequestUri,
                        Headers = headers,
                        Json = JObject.Parse(body),
                    }
                );
            }

            HttpStatusCode status;
            lock (_statuses)
            {
                status = _statuses.Count > 0 ? _statuses.Dequeue() : _last;
            }

            _received.Release();

            return new HttpResponseMessage(status)
            {
                Content = new StringContent(ResponseBody, Encoding.UTF8, "application/json"),
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _received.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

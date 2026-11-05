using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrispyWaffle.HttpClient;
using CrispyWaffle.HttpClient.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using HttpRequestOptions = CrispyWaffle.HttpClient.HttpRequestOptions;

namespace CrispyWaffle.Tests.HttpClient;

public class HttpClientWrapperTests
{
    [Fact]
    public async Task GetAsyncShouldDeserializeASuccessfulResponse()
    {
        // Arrange
        using var ok = Json(HttpStatusCode.OK, "{\"name\":\"waffle\"}");
        using var handler = new StubHandler(ok);
        var wrapper = CreateWrapper(handler, CreateOptions());

        // Act
        var result = await wrapper.GetAsync<Sample>("items/1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Name.Should().Be("waffle");
        handler.Calls.Should().Be(1);
    }

    [Fact]
    public async Task GetAsyncShouldRetryWhenTheServerAnswersTooManyRequests()
    {
        // Arrange
        using var tooManyRequests = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        using var ok = Json(HttpStatusCode.OK, "{\"name\":\"waffle\"}");
        using var handler = new StubHandler(tooManyRequests, ok);
        var wrapper = CreateWrapper(handler, CreateOptions());

        // Act
        var result = await wrapper.GetAsync<Sample>("items/1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Name.Should().Be("waffle");
        handler.Calls.Should().Be(2);
    }

    [Fact]
    public async Task GetAsyncShouldReturnAFailureForAnErrorStatus()
    {
        // Arrange
        using var notFound = Text(HttpStatusCode.NotFound, "missing");
        using var handler = new StubHandler(notFound);
        var wrapper = CreateWrapper(handler, CreateOptions());

        // Act
        var result = await wrapper.GetAsync<Sample>("items/1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Errors.Should().Equal("HTTP 404 Not Found", "missing");
        handler.Calls.Should().Be(1);
    }

    [Fact]
    public async Task GetAsyncShouldThrowForAnErrorStatusWhenThrowOnErrorIsSet()
    {
        // Arrange
        using var notFound = Text(HttpStatusCode.NotFound, "missing");
        using var handler = new StubHandler(notFound);
        var options = CreateOptions();
        options.ThrowOnError = true;
        var wrapper = CreateWrapper(handler, options);

        // Act
        Func<Task> act = () => wrapper.GetAsync<Sample>("items/1");

        // Assert
        var thrown = await act.Should().ThrowAsync<HttpClientException>();
        thrown.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        thrown.Which.ResponseContent.Should().Be("missing");
    }

    private static HttpRequestOptions CreateOptions() =>
        new()
        {
            BaseAddress = "https://example.test/",
            RetryBaseDelayMs = 1,
            MaxRetryDelayMs = 2,
        };

    private static HttpClientWrapper CreateWrapper(
        HttpMessageHandler handler,
        HttpRequestOptions options
    )
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory
            .CreateClient(Arg.Any<string>())
            .Returns(_ => new System.Net.Http.HttpClient(handler, false));

        return new HttpClientWrapper(factory, Options.Create(options));
    }

    private static HttpResponseMessage Json(HttpStatusCode statusCode, string json) =>
        new(statusCode) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static HttpResponseMessage Text(HttpStatusCode statusCode, string text) =>
        new(statusCode) { Content = new StringContent(text, Encoding.UTF8, "text/plain") };

    private sealed class Sample
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// A handler that answers each request with the next prepared response.
    /// </summary>
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses;

        public StubHandler(params HttpResponseMessage[] responses) =>
            _responses = new Queue<HttpResponseMessage>(responses);

        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Calls++;
            return Task.FromResult(_responses.Dequeue());
        }
    }
}

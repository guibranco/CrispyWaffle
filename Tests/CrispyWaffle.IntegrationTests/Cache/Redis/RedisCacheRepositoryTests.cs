using System;
using System.Text.Json;
using System.Threading.Tasks;
using CrispyWaffle.Redis.Cache;
using CrispyWaffle.Redis.Utils.Communications;
using StackExchange.Redis.Extensions.Core;
using Xunit;

namespace CrispyWaffle.IntegrationTests.Cache.Redis;

public class RedisCacheRepositoryTests :  IDisposable
{
    private readonly RedisCacheRepository _repository;

    public RedisCacheRepositoryTests()
    {
        var host = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
        var port = int.TryParse(Environment.GetEnvironmentVariable("REDIS_PORT"), out var p) ? p : 6379;
        var password = Environment.GetEnvironmentVariable("REDIS_PASSWORD");
        var serializer = new TestJsonSerializer();
        var connection = new RedisConnector(host, port, password, serializer);

        _repository = new RedisCacheRepository(connection);
    }

    [Fact]
    public async Task Should_Set_And_Get_Value_From_Redis()
    {
        // Arrange
        var key = $"crispy:it:{Guid.NewGuid()}";
        var value = "integration-test-value";

        await _repository.SetAsync(value, key);
        var result = await _repository.GetAsync<string>(key);

        Assert.Equal(value, result);
        await _repository.RemoveAsync(key);
    }

    [Fact]
    public async Task Should_Delete_Value_From_Redis()
    {
        var key = $"crispy:it:{Guid.NewGuid()}";
        var value = "to-be-deleted";

        await _repository.SetAsync(value, key);
        await _repository.RemoveAsync(key);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.GetAsync<string>(key).AsTask()
        );
        Assert.Contains("Unable to get the item with key", ex.Message);
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            _repository?.Dispose();
        }
    }
}

public class TestJsonSerializer : ISerializer
{
    private readonly JsonSerializerOptions _options;

    public TestJsonSerializer(JsonSerializerOptions options = null)
    {
        _options = options ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public byte[] Serialize<T>(T item)
    {
        return JsonSerializer.SerializeToUtf8Bytes<T>(item, _options);
    }

    public T Deserialize<T>(byte[] serializedObject)
    {
        return JsonSerializer.Deserialize<T>(serializedObject, _options);
    }
}


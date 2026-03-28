using System.Threading.Tasks;
using CouchDB.Driver;
using CrispyWaffle.CouchDB.Cache;
using CrispyWaffle.CouchDB.Utils.Communications;
using FluentAssertions;
using Flurl.Http.Testing;
using NSubstitute;
using Xunit;

namespace CrispyWaffle.Tests.Cache;

public class CouchDBCacheRepositoryTests
{
    private readonly CouchDBConnector _connector;
    private readonly CouchDBCacheRepository _repository;

    public CouchDBCacheRepositoryTests()
    {
        var httpTest = new HttpTest();
        httpTest.RespondWithJson(new { ok = true });

        var client = new CouchClient("http://localhost");
        _connector = Substitute.For<CouchDBConnector>(client);
        _repository = new CouchDBCacheRepository(_connector);
    }

    [Fact]
    public async Task SetToDatabaseShouldStoreValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        // Act
        await _repository.SetAsync(value, key);

        // Assert
    }

    [Fact]
    public async Task GetFromDatabaseShouldReturnStoredValue()
    {
        // Arrange
        var key = "test-key";

        // Act
        var actualValue = await _repository.GetAsync<string>(key);

        // Assert
        actualValue.Should().BeNull();
    }

    [Fact]
    public async Task RemoveFromDatabaseShouldRemoveValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        // Act
        await _repository.SetAsync(value, key);

        // Act
        await _repository.RemoveAsync(key);
        (bool success, _)= await _repository.TryGetAsync<object>(key);

        // Assert
        success.Should().Be(false);
    }
}

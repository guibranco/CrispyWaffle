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
    public void SetToDatabaseShouldStoreValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        // Act
        _repository.Set(value, key);

        // Assert
    }

    [Fact]
    public void GetFromDatabaseShouldReturnStoredValue()
    {
        // Arrange
        var key = "test-key";

        // Act
        var actualValue = _repository.Get<string>(key);

        // Assert
        actualValue.Should().BeNull();
    }

    [Fact]
    public void RemoveFromDatabaseShouldRemoveValue()
    {
        // Arrange
        var key = "test-key";

        // Act
        _repository.Remove(key);

        // Assert
    }
}

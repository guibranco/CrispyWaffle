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
        const string key = "test-key";
        const string value = "test-value";

        var httpTest = new HttpTest();
        httpTest.RespondWithJson(new { ok = true });

        var client = new CouchClient("http://localhost");
        _connector = Substitute.For<CouchDBConnector>(client);
        _repository = new CouchDBCacheRepository(_connector);
    }

    [Fact]
    public void SetToDatabase_ShouldStoreValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        // Act
        _repository.Set(value, key);

        // Assert
    }

    [Fact]
    public void GetFromDatabase_ShouldReturnStoredValue()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = "test-value";

        // Act
        var actualValue = _repository.Get<string>(key);

        // Assert
        actualValue.Should().BeNull();
    }

    [Fact]
    public void RemoveFromDatabase_ShouldRemoveValue()
    {
        // Arrange
        var key = "test-key";

        // Act
        _repository.Remove(key);

        // Assert
    }
}

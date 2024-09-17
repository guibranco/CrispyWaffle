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
        const string key = "test-key";
        const string value = "test-value";

        var httpTest = new HttpTest();
        httpTest.RespondWithJson(new { ok = true });

        var client = new CouchClient("http://localhost");
        _connector = Substitute.For<CouchDBConnector>(client);
        _repository = new CouchDBCacheRepository(_connector);
    }

    /// <summary>
    /// Tests the SetAsync method of the repository to ensure it correctly stores a value associated with a given key.
    /// </summary>
    /// <remarks>
    /// This unit test verifies that when a value is set in the repository using a specified key,
    /// the value is stored correctly. The test uses the Arrange-Act-Assert pattern, where:
    /// - In the Arrange phase, a key and a value are defined.
    /// - In the Act phase, the SetAsync method is called to store the value with the specified key.
    /// - The Assert phase is currently empty, indicating that further assertions may be needed
    /// to confirm that the value was stored as expected.
    /// This test is marked with the [Fact] attribute, indicating that it is a unit test that can be run by a test runner.
    /// </remarks>
    [Fact]
    public async Task SetToDatabase_ShouldStoreValueAsync()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        // Act
        await _repository.SetAsync(value, key);

        // Assert
    }

    /// <summary>
    /// Tests the retrieval of a stored value from the database.
    /// </summary>
    /// <remarks>
    /// This test method verifies that the <see cref="_repository.Get{T}"/> method correctly retrieves a value associated with a specified key from the database.
    /// It sets up a test scenario where a key is defined, and an expected value is specified.
    /// The method then calls the GetAsync method with the key and asserts that the actual value returned is null, indicating that no value is stored for the given key.
    /// This is useful for ensuring that the database behaves as expected when attempting to retrieve a value that does not exist.
    /// </remarks>
    [Fact]
    public async Task GetFromDatabase_ShouldReturnStoredValueAsync()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = "test-value";

        // Act
        var actualValue = await _repository.GetAsync<string>(key);

        // Assert
        actualValue.Should().BeNull();
    }

    /// <summary>
    /// Tests the removal of a value from the database.
    /// </summary>
    /// <remarks>
    /// This unit test verifies that the method <c>Remove</c> of the repository correctly removes an entry associated with the specified key from the database.
    /// The test is structured in three main phases:
    /// - **Arrange**: Sets up the necessary preconditions and inputs for the test. In this case, a key is defined that will be used to remove an entry from the database.
    /// - **Act**: Calls the method under test, which is expected to perform the removal operation.
    /// - **Assert**: Although there are no assertions in this snippet, typically this phase would check that the entry has been successfully removed from the database.
    /// This test is marked with the <c>[Fact]</c> attribute, indicating that it is a test method that should be executed by the testing framework.
    /// </remarks>
    [Fact]
    public async Task RemoveFromDatabase_ShouldRemoveValueAsync()
    {
        // Arrange
        var key = "test-key";

        // Act
        await _repository.RemoveAsync(key);

        // Assert
    }
}

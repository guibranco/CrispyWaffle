using System;
using System.Threading.Tasks;
using CrispyWaffle.Configuration;
using CrispyWaffle.CouchDB;
using CrispyWaffle.CouchDB.DTOs;
using Xunit;

namespace CrispyWaffle.Tests.Cache;

[Collection("Sequential")]
public class CouchDBCacheRepositoryTests : IDisposable
{
    private readonly CouchDBCacheRepository _repo;
    public CouchDBCacheRepositoryTests()
    {
        var conn = new Connection();
        conn.Host = "http://localhost";
        conn.Port = 5984;
        conn.Credentials.Username = "Admin";
        conn.Credentials.Password = "myP@ssw0rd";

        _repo = new CouchDBCacheRepository(conn, AuthType.Basic);
    }

    [Fact]
    public void GetAndSetCouchDocTest()
    {
        var doc = new CouchDoc();

        _repo.Set(doc, Guid.NewGuid().ToString());

        var docDB = _repo.Get<CouchDoc>(doc.Key);

        Assert.True(doc.Key == docDB.Key);

        _repo.Remove(doc.Key);
    }

    [Fact]
    public void GetAndSetSpecificTest()
    {
        var docOne = new Car("MakerOne");

        _repo.SetSpecific(docOne, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        var docTwo = new Car("MakerTwo");

        _repo.SetSpecific(docTwo, Guid.NewGuid().ToString());

        var docDB = _repo.GetSpecific<Car>(docOne.Key);

        Assert.True(docOne.Key == docDB.Key
            && docOne.SubKey == docDB.SubKey
            && docOne.Maker == "MakerOne");

        docDB = _repo.GetSpecific<Car>(docTwo.Key);

        Assert.True(docTwo.Key == docDB.Key
            && docTwo.Maker == "MakerTwo");

        _repo.RemoveSpecific<Car>(docOne.Key);
        _repo.RemoveSpecific<Car>(docTwo.Key);
    }

    [Fact]
    public void RemoveCouchDocTest()
    {
        var doc = new CouchDoc();

        _repo.Set(doc, Guid.NewGuid().ToString());

        _repo.Remove(doc.Key);

        var docDB = _repo.Get<CouchDoc>(doc.Key);

        Assert.True(docDB == default);
    }

    [Fact]
    public void RemoveSpecificTest()
    {
        var doc = new Car("Maker");

        _repo.SetSpecific(doc, Guid.NewGuid().ToString());

        _repo.RemoveSpecific<Car>(doc.Key);

        var docDB = _repo.Get<CouchDoc>(doc.Key);

        Assert.True(docDB == default);
    }

    [Fact]
    public void DatabaseClearTest()
    {
        _repo.Set(new CouchDoc(), Guid.NewGuid().ToString());
        _repo.Set(new CouchDoc(), Guid.NewGuid().ToString());
        _repo.Set(new CouchDoc(), Guid.NewGuid().ToString());
        _repo.Set(new CouchDoc(), Guid.NewGuid().ToString());

        _repo.Clear();

        var count = _repo.GetDocCount<CouchDoc>();

        Assert.True(count == 0);
    }
    /// <summary>
    /// Tests the Time-To-Live (TTL) functionality of the CouchDB repository.
    /// </summary>
    /// <remarks>
    /// This asynchronous test method verifies that a document stored in the CouchDB repository expires after a specified time period.
    /// It creates a new document with a unique key, sets it in the repository with a TTL of 5 seconds, and then retrieves it to ensure it exists.
    /// After waiting for 6 seconds, it attempts to retrieve the document again, expecting it to be null, indicating that it has expired.
    /// This test ensures that the TTL feature of the repository is functioning as intended.
    /// </remarks>
    /// <exception cref="System.Exception">
    /// Throws an exception if the assertions fail, indicating that the expected behavior of the TTL functionality is not met.
    /// </exception>
    [Fact]
    public async Task TTLGetTest()
    {
        var doc = new CouchDoc()
        {
            Key = Guid.NewGuid().ToString()
        };

        _repo.Set(new CouchDoc(), doc.Key, new TimeSpan(0, 0, 5));
        var fromDB = _repo.Get<CouchDoc>(doc.Key);

        Assert.True(doc.Key == fromDB.Key);

        await Task.Delay(6000);

        fromDB = _repo.Get<CouchDoc>(doc.Key);

        Assert.True(fromDB == null);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _repo?.Dispose();
        }
    }
}

public class Car : CouchDoc
{
    public Car(string maker)
    {
        Maker = maker;
    }

    public string Maker { get; set; }
}

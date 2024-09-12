using System;
using System.Threading.Tasks;
using CrispyWaffle.Configuration;
using CrispyWaffle.CouchDB;
using CrispyWaffle.CouchDB.DTOs;
using Xunit;

namespace CrispyWaffle.Tests.Cache;

public class CouchDBCacheRepositoryTests : IDisposable
{
    private readonly CouchDBCacheRepository _repository;

    public CouchDBCacheRepositoryTests()
    {
        var conn = new Connection { Host = "http://localhost", Port = 5984, Credentials =
            {
                Username = "Admin", Password = "myP@ssw0rd"
            }
        };

        _repository = new CouchDBCacheRepository(conn, AuthType.Basic);
    }

    [Fact]
    public void GetAndSetCouchDocTest()
    {
        var doc = new CouchDoc();

        _repository.Set(doc, Guid.NewGuid().ToString());

        var docDB = _repository.Get<CouchDoc>(doc.Key);

        Assert.True(doc.Key == docDB.Key);

        _repository.Remove(doc.Key);
    }

    [Fact]
    public void GetAndSetSpecificTest()
    {
        var docOne = new Car("MakerOne");

        _repository.SetSpecific(docOne, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        var docTwo = new Car("MakerTwo");

        _repository.SetSpecific(docTwo, Guid.NewGuid().ToString());

        var docDB = _repository.GetSpecific<Car>(docOne.Key);

        Assert.True(
            docOne.Key == docDB.Key && docOne.SubKey == docDB.SubKey && docOne.Maker == "MakerOne"
        );

        docDB = _repository.GetSpecific<Car>(docTwo.Key);

        Assert.True(docTwo.Key == docDB.Key && docTwo.Maker == "MakerTwo");

        _repository.RemoveSpecific<Car>(docOne.Key);
        _repository.RemoveSpecific<Car>(docTwo.Key);
    }

    [Fact]
    public void RemoveCouchDocTest()
    {
        var doc = new CouchDoc();

        _repository.Set(doc, Guid.NewGuid().ToString());

        _repository.Remove(doc.Key);

        var docDB = _repository.Get<CouchDoc>(doc.Key);

        Assert.True(docDB == default);
    }

    [Fact]
    public void RemoveSpecificTest()
    {
        var doc = new Car("Maker");

        _repository.SetSpecific(doc, Guid.NewGuid().ToString());

        _repository.RemoveSpecific<Car>(doc.Key);

        var docDB = _repository.Get<CouchDoc>(doc.Key);

        Assert.True(docDB == default);
    }

    [Fact]
    public void DatabaseClearTest()
    {
        _repository.Set(new CouchDoc(), Guid.NewGuid().ToString());
        _repository.Set(new CouchDoc(), Guid.NewGuid().ToString());
        _repository.Set(new CouchDoc(), Guid.NewGuid().ToString());
        _repository.Set(new CouchDoc(), Guid.NewGuid().ToString());

        _repository.Clear();

        var count = _repository.GetDocCount<CouchDoc>();

        Assert.True(count == 0);
    }

    [Fact]
    public async Task TTLGetTest()
    {
        var doc = new CouchDoc() { Key = Guid.NewGuid().ToString() };

        _repository.Set(new CouchDoc(), doc.Key, new TimeSpan(0, 0, 5));
        var fromDB = _repository.Get<CouchDoc>(doc.Key);

        Assert.True(doc.Key == fromDB.Key);

        await Task.Delay(6000);

        fromDB = _repository.Get<CouchDoc>(doc.Key);

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
            _repository?.Dispose();
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

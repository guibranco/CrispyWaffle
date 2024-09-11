using System;
using System.Threading;
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

        Assert.True(
            docOne.Key == docDB.Key && docOne.SubKey == docDB.SubKey && docOne.Maker == "MakerOne"
        );

        docDB = _repo.GetSpecific<Car>(docTwo.Key);

        Assert.True(docTwo.Key == docDB.Key && docTwo.Maker == "MakerTwo");

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

    [Fact]
    public void TTLGetTest()
    {
        var doc = new CouchDoc() { Key = Guid.NewGuid().ToString() };

        _repo.Set(new CouchDoc(), doc.Key, new TimeSpan(0, 0, 5));
        var fromDB = _repo.Get<CouchDoc>(doc.Key);

        Assert.True(doc.Key == fromDB.Key);

        Thread.Sleep(6000);

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

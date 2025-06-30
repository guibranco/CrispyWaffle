using System;
using System.Threading.Tasks;
using CrispyWaffle.Configuration;
using CrispyWaffle.CouchDB.Cache;
using CrispyWaffle.CouchDB.Utils.Communications;
using Xunit;

namespace CrispyWaffle.IntegrationTests.Cache;

public class CouchDBCacheRepositoryTests : IDisposable
{
    private readonly CouchDBCacheRepository _repository;

    public CouchDBCacheRepositoryTests()
    {
        var connection = new Connection
        {
            Host = "http://localhost",
            Port = 5984,
            Credentials = { Username = "Admin", Password = "myP@ssw0rd" },
        };
        var connector = new CouchDBConnector(connection);
        _repository = new CouchDBCacheRepository(connector);
    }

    [Fact]
    public async Task GetAndSetAsyncCouchDocTest()
    {
        var doc = new CouchDBCacheDocument();

        await _repository.SetAsync(doc, Guid.NewGuid().ToString());

        var docDB = await _repository.GetAsync<CouchDBCacheDocument>(doc.Key);

        Assert.True(doc.Key == docDB.Key);

        await _repository.RemoveAsync(doc.Key);
    }

    [Fact]
    public async Task GetAndSetSpecificAsyncTest()
    {
        var docOne = new Car("MakerOne");

        await _repository.SetSpecificAsync(docOne, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        var docTwo = new Car("MakerTwo");

        await _repository.SetSpecificAsync(docTwo, Guid.NewGuid().ToString());

        var docDB = await _repository.GetSpecificAsync<Car>(docOne.Key);

        Assert.True(
            docOne.Key == docDB.Key && docOne.SubKey == docDB.SubKey && docOne.Maker == "MakerOne"
        );

        docDB = await _repository.GetSpecificAsync<Car>(docTwo.Key);

        Assert.True(docTwo.Key == docDB.Key && docTwo.Maker == "MakerTwo");

        await _repository.RemoveSpecificAsync<Car>(docOne.Key);
        await _repository.RemoveSpecificAsync<Car>(docTwo.Key);
    }

    [Fact]
    public async Task RemoveAsyncCouchDocTest()
    {
        var doc = new CouchDBCacheDocument();

        await _repository.SetAsync(doc, Guid.NewGuid().ToString());

        await _repository.RemoveAsync(doc.Key);

        var docDB = await _repository.GetAsync<CouchDBCacheDocument>(doc.Key);

        Assert.True(docDB == default);
    }

    [Fact]
    public async Task RemoveSpecificAsyncTest()
    {
        var doc = new Car("Maker");

        await _repository.SetSpecificAsync(doc, Guid.NewGuid().ToString());

        await _repository.RemoveSpecificAsync<Car>(doc.Key);

        var docDB = await _repository.GetAsync<CouchDBCacheDocument>(doc.Key);

        Assert.True(docDB == default);
    }

    [Fact]
    public async Task DatabaseClearTest()
    {
        var task1 = _repository.SetAsync(new CouchDBCacheDocument(), Guid.NewGuid().ToString()).AsTask();
        var task2 = _repository.SetAsync(new CouchDBCacheDocument(), Guid.NewGuid().ToString()).AsTask();
        var task3 = _repository.SetAsync(new CouchDBCacheDocument(), Guid.NewGuid().ToString()).AsTask();
        var task4 = _repository.SetAsync(new CouchDBCacheDocument(), Guid.NewGuid().ToString()).AsTask();

        await Task.WhenAll(task1, task2, task3, task4);

        await _repository.Clear();

        var count = await _repository.GetDocCount<CouchDBCacheDocument>();

        Assert.True(count == 0);
    }

    [Fact]
    public async Task TTLGetTest()
    {
        var doc = new CouchDBCacheDocument() { Key = Guid.NewGuid().ToString() };

        await _repository.SetAsync(new CouchDBCacheDocument(), doc.Key, new TimeSpan(0, 0, 5));
        var fromDB = await _repository.GetAsync<CouchDBCacheDocument>(doc.Key);

        Assert.True(doc.Key == fromDB.Key);

        await Task.Delay(6000);

        fromDB = await _repository.GetAsync<CouchDBCacheDocument>(doc.Key);

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

public class Car : CouchDBCacheDocument
{
    public Car(string maker) => Maker = maker;

    public string Maker { get; set; }
}

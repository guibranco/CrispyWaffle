using System;
using System.Threading.Tasks;
using CrispyWaffle.Configuration;
using CrispyWaffle.ElasticSearch.Helpers;
using CrispyWaffle.ElasticSearch.Utils.Communications;
using Elastic.Clients.Elasticsearch;
using NSubstitute;
using Xunit;

namespace CrispyWaffle.Tests.ElasticSearch;

public sealed class ElasticWrapperTests
{
    private static ElasticWrapper CreateWrapper()
    {
        var connection = Substitute.For<IConnection>();
        connection.Host.Returns("localhost");
        connection.Port.Returns(1);
        var connector = new ElasticConnector(connection, "default-idx");
        return new ElasticWrapper(connector);
    }

    [Theory]
    [InlineData("42")]
    [InlineData(42)]
    [InlineData(42L)]
    public async Task GetDocumentAsyncDoesNotThrowInvalidCastExceptionForStringOrNumericIds(object id)
    {
        var wrapper = CreateWrapper();

        var ex = await Record.ExceptionAsync(() => wrapper.GetDocumentAsync<SampleDoc>(id));
        Assert.IsNotType<InvalidCastException>(ex);
    }

    [Fact]
    public async Task GetDocumentVariantsAcceptStringNumericGuidAndIdWithoutInvalidCastException()
    {
        var wrapper = CreateWrapper();

        var resolverEx = await Record.ExceptionAsync(() => wrapper.GetDocumentWithResolverAsync<SampleDoc>(42L));
        Assert.IsNotType<InvalidCastException>(resolverEx);

        var fromEx = await Record.ExceptionAsync(() => wrapper.GetDocumentFromAsync<SampleDoc>(Guid.NewGuid(), "custom-idx"));
        Assert.IsNotType<InvalidCastException>(fromEx);

        var idEx = await Record.ExceptionAsync(() => wrapper.GetDocumentAsync<SampleDoc>(new Id("doc-1")));
        Assert.IsNotType<InvalidCastException>(idEx);
    }

    [Fact]
    public async Task GetDocumentAsyncThrowsArgumentNullExceptionWhenIdIsNull()
    {
        var wrapper = CreateWrapper();

        await Assert.ThrowsAsync<ArgumentNullException>(() => wrapper.GetDocumentAsync<SampleDoc>(null));
    }

    [IndexName("sample-docs")]
    public sealed class SampleDoc : IIndexable
    {
        public string Id { get; set; }
    }
}

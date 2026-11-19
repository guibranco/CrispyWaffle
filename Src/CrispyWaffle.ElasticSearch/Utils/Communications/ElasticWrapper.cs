using System;
using System.Globalization;
using System.Threading.Tasks;
using CrispyWaffle.ElasticSearch.Helpers;
using Elastic.Clients.Elasticsearch;

namespace CrispyWaffle.ElasticSearch.Utils.Communications;

/// <summary>
/// The Elasticsearch wrapper class.
/// </summary>
public sealed class ElasticWrapper
{
    /// <summary>
    /// The elastic client.
    /// </summary>
    private readonly ElasticsearchClient _elastic;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElasticWrapper" /> class.
    /// </summary>
    /// <param name="elastic">The elastic.</param>
    public ElasticWrapper(ElasticConnector elastic) => _elastic = elastic.Client;

    /// <summary>
    /// Indexes the exists.
    /// </summary>
    /// <typeparam name="T">The type to be used.</typeparam>
    /// <returns><c>true</c> if exists, <c>false</c> otherwise.</returns>
    public bool IndexExists<T>()
        where T : class, IIndexable, new() =>
        _elastic.Indices.ExistsAsync(Helpers.Extensions.GetIndexName<T>()).Result.Exists;

    /// <summary>
    /// Indexes the exists.
    /// </summary>
    /// <typeparam name="T">The type to be used.</typeparam>
    /// <returns><c>true</c> if exists, <c>false</c> otherwise.</returns>
    public async Task<bool> IndexExistsAsync<T>()
        where T : class, IIndexable, new() =>
        (await _elastic.Indices.ExistsAsync(Helpers.Extensions.GetIndexName<T>())).Exists;

    /// <summary>
    /// Converts the provided identifier object into an Elasticsearch <see cref="Id"/>.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>An <see cref="Id"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <c>null</c>.</exception>
    private static Id ToId(object id) =>
        id switch
        {
            null => throw new ArgumentNullException(nameof(id)),
            Id elasticId => elasticId,
            string s => s,
            long l => l,
            Guid g => g,
            IFormattable formattable => new Id(
                formattable.ToString(null, CultureInfo.InvariantCulture)
            ),
            _ => new Id(id.ToString()),
        };

    /// <summary>
    /// Gets the document.
    /// </summary>
    /// <typeparam name="T">The type to be used.</typeparam>
    /// <param name="id">The identifier.</param>
    /// <returns>T.</returns>
    public T GetDocument<T>(object id)
        where T : class, new() => _elastic.GetAsync<T>(ToId(id)).Result.Source;

    /// <summary>
    /// Gets the document.
    /// </summary>
    /// <typeparam name="T">The type to be used.</typeparam>
    /// <param name="id">The identifier.</param>
    /// <returns>T.</returns>
    public async Task<T> GetDocumentAsync<T>(object id)
        where T : class, new() => (await _elastic.GetAsync<T>(ToId(id))).Source;

    /// <summary>
    /// Gets the document with resolver.
    /// </summary>
    /// <typeparam name="T">The type to be used.</typeparam>
    /// <param name="id">The identifier.</param>
    /// <returns>T.</returns>
    public T GetDocumentWithResolver<T>(object id)
        where T : class, IIndexable, new() =>
        _elastic
            .GetAsync<T>(new GetRequest(Helpers.Extensions.GetIndexName<T>(), ToId(id)))
            .Result.Source;

    /// <summary>
    /// Gets the document with resolver.
    /// </summary>
    /// <typeparam name="T">The type to be used.</typeparam>
    /// <param name="id">The identifier.</param>
    /// <returns>T.</returns>
    public async Task<T> GetDocumentWithResolverAsync<T>(object id)
        where T : class, IIndexable, new() =>
        (
            await _elastic.GetAsync<T>(
                new GetRequest(Helpers.Extensions.GetIndexName<T>(), ToId(id))
            )
        ).Source;

    /// <summary>
    /// Gets the document from.
    /// </summary>
    /// <typeparam name="T">The type to be used.</typeparam>
    /// <param name="id">The identifier.</param>
    /// <param name="indexName">Name of the index.</param>
    /// <returns>T.</returns>
    public T GetDocumentFrom<T>(object id, string indexName)
        where T : class, new() =>
        _elastic.GetAsync<T>(new GetRequest(indexName, ToId(id))).Result.Source;

    /// <summary>
    /// Gets the document from.
    /// </summary>
    /// <typeparam name="T">The type to be used.</typeparam>
    /// <param name="id">The identifier.</param>
    /// <param name="indexName">Name of the index.</param>
    /// <returns>T.</returns>
    public async Task<T> GetDocumentFromAsync<T>(object id, string indexName)
        where T : class, new() =>
        (await _elastic.GetAsync<T>(new GetRequest(indexName, ToId(id)))).Source;

    /// <summary>
    /// Sets the document.
    /// </summary>
    /// <typeparam name="T">The type to be used.</typeparam>
    /// <param name="document">The document.</param>
    public void SetDocument<T>(T document)
        where T : class, new() => _elastic.IndexAsync(document).Wait();

    /// <summary>
    /// Sets the document.
    /// </summary>
    /// <typeparam name="T">The type to be used.</typeparam>
    /// <param name="document">The document.</param>
    /// <returns>A task to be awaited on.</returns>
    public async Task SetDocumentAsync<T>(T document)
        where T : class, new() => await _elastic.IndexAsync(document);

    /// <summary>
    /// Sets the document with resolver.
    /// </summary>
    /// <typeparam name="T">The type to be used.</typeparam>
    /// <param name="document">The document.</param>
    public void SetDocumentWithResolver<T>(T document)
        where T : class, IIndexable, new() =>
        _elastic.IndexAsync(document, i => i.Index(Helpers.Extensions.GetIndexName<T>())).Wait();

    /// <summary>
    /// Sets the document with resolver.
    /// </summary>
    /// <typeparam name="T">The type to be used.</typeparam>
    /// <param name="document">The document.</param>
    /// <returns>A task to be awaited on.</returns>
    public async Task SetDocumentWithResolverAsync<T>(T document)
        where T : class, IIndexable, new() =>
        await _elastic.IndexAsync(document, i => i.Index(Helpers.Extensions.GetIndexName<T>()));

    /// <summary>
    /// Sets the document to.
    /// </summary>
    /// <typeparam name="T">The type to be used.</typeparam>
    /// <param name="document">The document.</param>
    /// <param name="indexName">Name of the index.</param>
    public void SetDocumentTo<T>(T document, string indexName)
        where T : class, new() => _elastic.IndexAsync(document, i => i.Index(indexName)).Wait();

    /// <summary>
    /// Sets the document to.
    /// </summary>
    /// <typeparam name="T">The type to be used.</typeparam>
    /// <param name="document">The document.</param>
    /// <param name="indexName">Name of the index.</param>
    /// <returns>A task to be awaited on.</returns>
    public async Task SetDocumentToAsync<T>(T document, string indexName)
        where T : class, new() => await _elastic.IndexAsync(document, i => i.Index(indexName));
}

// ***********************************************************************
// Assembly         : CrispyWaffle.ElasticSearch
// Author           : Guilherme Branco Stracini
// Created          : 10/09/2022
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 10/09/2022
// ***********************************************************************
// <copyright file="ElasticWrapper.cs" company="Guilherme Branco Stracini ME">
//     © 2022 Guilherme Branco Stracini, All Rights Reserved
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.ElasticSearch.Utils.Communications
{
    using CrispyWaffle.ElasticSearch.Helpers;
    using Nest;

    /// <summary>
    /// The Elastic Search wrapper class.
    /// </summary>
    public sealed class ElasticWrapper
    {
        #region Private fields

        /// <summary>
        /// The elastic
        /// </summary>
        private readonly ElasticClient _elastic;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticWrapper" /> class.
        /// </summary>
        /// <param name="elastic">The elastic.</param>
        public ElasticWrapper(ElasticConnector elastic) => _elastic = elastic.Client;

        #endregion

        #region Public methods

        /// <summary>
        /// Indexes the exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise.</returns>
        public bool IndexExists<T>()
            where T : class, IIndexable, new() =>
            _elastic.Indices.Exists(Extensions.GetIndexName<T>()).Exists;

        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns>T.</returns>

        public T GetDocument<T>(object id) where T : class, new() => _elastic.Get(DocumentPath<T>.Id((Id)id)).Source;

        /// <summary>
        /// Gets the document with resolver.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns>T.</returns>

        public T GetDocumentWithResolver<T>(object id) where T : class, IIndexable, new() => _elastic.Get<T>(new GetRequest(Extensions.GetIndexName<T>(), (Id)id)).Source;

        /// <summary>
        /// Gets the document from.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <returns>T.</returns>

        public T GetDocumentFrom<T>(object id, string indexName) where T : class, new() => _elastic.Get<T>(new GetRequest(indexName, (Id)id)).Source;

        /// <summary>
        /// Sets the document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>

        public void SetDocument<T>(T document) where T : class, new() => _elastic.IndexDocument(document);

        /// <summary>
        /// Sets the document with resolver.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        public void SetDocumentWithResolver<T>(T document) where T : class, IIndexable, new() => _elastic.Index(document, i => i.Index(Extensions.GetIndexName<T>()));

        /// <summary>
        /// Sets the document to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        /// <param name="indexName">Name of the index.</param>

        public void SetDocumentTo<T>(T document, string indexName) where T : class, new() => _elastic.Index(document, i => i.Index(indexName));

        #endregion
    }
}

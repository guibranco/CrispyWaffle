using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CrispyWaffle.Composition;
using CrispyWaffle.ElasticSearch.Utils.Communications;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;

namespace CrispyWaffle.ElasticSearch.Helpers
{
    /// <summary>
    /// The extensions class.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// The connector.
        /// </summary>
        private static readonly ElasticConnector _connector =
            ServiceLocator.Resolve<ElasticConnector>();

        /// <summary>
        /// Gets the name of the index.
        /// </summary>
        /// <typeparam name="T">The type to be indexed.</typeparam>
        /// <returns>System.String.</returns>
        public static string GetIndexName<T>()
            where T : class, IIndexable, new()
        {
            var type = typeof(T);
            return
                type.GetCustomAttributes(typeof(IndexNameAttribute), true)
                    is IndexNameAttribute[] attributes
                && attributes.Length == 1
                ? attributes[0].IndexName
                : type.Name.ToLowerInvariant().Replace(@" ", @"-");
        }

        /// <summary>
        /// Adds the alias.
        /// </summary>
        /// <typeparam name="T">The type to be indexed.</typeparam>
        /// <param name="index">The index.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <returns>T.</returns>
        public static async Task<T> AliasAsync<T>(
            this T index,
            string alias,
            string indexName = null
        )
            where T : class, IIndexable, new()
        {
            if (string.IsNullOrWhiteSpace(indexName))
            {
                indexName = GetIndexName<T>();
            }

            await _connector.Client.Indices.UpdateAliasesAsync(aliases =>
                aliases.Actions(actions =>
                    actions.Add(new AddAction() { Index = indexName, Alias = alias })
                )
            );

            return index;
        }

        /// <summary>
        /// Deletes the specified index.
        /// </summary>
        /// <typeparam name="T">The type to be indexed.</typeparam>
        /// <param name="index">The index.</param>
        /// <returns>T.</returns>
        public static async Task<T> DeleteAsync<T>(this T index)
            where T : class, IIndexable, new()
        {
            var indexName = GetIndexName<T>();
            if ((await _connector.Client.Indices.ExistsAsync(indexName)).Exists)
            {
                await _connector.Client.Indices.DeleteAsync(indexName);
            }

            return index;
        }

        /// <summary>
        /// Deletes the document by query.
        /// </summary>
        /// <typeparam name="T">The type to be indexed.</typeparam>
        /// <param name="index">The index.</param>
        /// <param name="field">The field.</param>
        /// <param name="indexPattern">The index pattern.</param>
        /// <param name="daysBefore">The days before.</param>
        /// <returns>A task to be awaited on.</returns>
        public static async Task<long?> DeleteByQueryAsync<T>(
            this T index,
            Expression<Func<T, object>> field,
            string indexPattern,
            int daysBefore
        )
            where T : class, new()
        {
            var result = await _connector.Client.DeleteByQueryAsync<T>(
                indexPattern,
                d =>
                    d.Query(q =>
                        q.Range(r =>
                            r.DateRange(dr =>
                                dr.Field(field).Lt(DateMath.Now.Subtract($@"{daysBefore}d"))
                            )
                        )
                    )
            );

            return result.Deleted;
        }
    }
}

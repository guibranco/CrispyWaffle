// ***********************************************************************
// Assembly         : CrispyWaffle.ElasticSearch
// Author           : Guilherme Branco Stracini
// Created          : 10/09/2022
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 10/09/2022
// ***********************************************************************
// <copyright file="Extensions.cs" company="Guilherme Branco Stracini ME">
//     © 2022 Guilherme Branco Stracini, All Rights Reserved
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.ElasticSearch.Helpers
{
    using System;
    using System.Linq.Expressions;
    using CrispyWaffle.Composition;
    using CrispyWaffle.ElasticSearch.Utils.Communications;
    using Nest;

    /// <summary>
    /// The extensions class.
    /// </summary>
    /// <summary>
    /// Class Extensions.
    /// </summary>
    public static class Extensions
    {
        #region Private fields

        /// <summary>
        /// The connector
        /// </summary>
        /// <summary>
        /// The connector
        /// </summary>
        private static readonly ElasticConnector _connector = ServiceLocator.Resolve<ElasticConnector>();

        #endregion

        #region Public methods

        /// <summary>
        /// Gets the name of the index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>System.String.</returns>

        public static string GetIndexName<T>()
            where T : class, IIndexable, new()
        {
            var type = typeof(T);
            return type.GetCustomAttributes(typeof(IndexNameAttribute), true)
                       is IndexNameAttribute[] attributes && attributes.Length == 1
                       ? attributes[0].IndexName
                       : type.Name.ToLower().Replace(@" ", @"-");
        }

        /// <summary>
        /// Adds the alias.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">The index.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <returns>T.</returns>

        public static T Alias<T>(this T index, string alias, string indexName = null)
            where T : class, IIndexable, new()
        {
            if (string.IsNullOrWhiteSpace(indexName))
            {
                indexName = GetIndexName<T>();
            }

            _connector.Client.Indices.BulkAlias(a => a.Add(add => add.Index(indexName).Alias(alias)));
            return index;
        }

        /// <summary>
        /// Deletes the specified index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">The index.</param>
        /// <returns>T.</returns>

        public static T Delete<T>(this T index)
            where T : class, IIndexable, new()
        {
            var indexName = GetIndexName<T>();
            if (_connector.Client.Indices.Exists(indexName).Exists)
            {
                _connector.Client.Indices.Delete(indexName);
            }

            return index;
        }

        /// <summary>
        /// Automatics the map.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">The index.</param>
        /// <returns>T.</returns>

        public static T AutoMap<T>(this T index)
            where T : class, IIndexable, new()
        {
            var indexName = GetIndexName<T>();
            if (!_connector.Client.Indices.Exists(indexName).Exists)
            {
                _connector.Client.Indices.Create(indexName, descriptor => descriptor.Map(ms => ms.AutoMap<T>()));
            }

            return index;
        }

        /// <summary>
        /// Deletes the by query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">The index.</param>
        /// <param name="field">The field.</param>
        /// <param name="indexPattern">The index pattern.</param>
        /// <param name="daysBefore">The days before.</param>
        /// <returns>System.Int64.</returns>
        public static long DeleteByQuery<T>(this T index,
            Expression<Func<T, object>> field,
            string indexPattern,
            int daysBefore) where T : class, new()
        {
            var result = _connector
                .Client
                .DeleteByQuery<T>(d =>
                    d.Index(indexPattern)
                        .Query(q =>
                            q.DateRange(g =>
                                g.Field(field)
                                .LessThan(DateMath.Now.Subtract($@"{daysBefore}d")))));

            return result.Deleted;

        }
        #endregion
    }
}

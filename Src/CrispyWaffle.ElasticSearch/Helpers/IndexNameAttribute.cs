using System;

namespace CrispyWaffle.ElasticSearch.Helpers
{
    /// <summary>
    /// The index name attribute.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class IndexNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexNameAttribute" /> class.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        public IndexNameAttribute(string indexName) => IndexName = indexName;

        /// <summary>
        /// Gets the name of the index.
        /// </summary>
        /// <value>The name of the index.</value>
        public string IndexName { get; }
    }
}

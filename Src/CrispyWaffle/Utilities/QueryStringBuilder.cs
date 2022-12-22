namespace CrispyWaffle.Utilities
{
    using Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Web;

    /// <summary>
    /// A chainable query string helper class. Example usage : string strQuery =
    /// QueryString.Current.Add("id", "179").ToString();
    /// string strQuery = new QueryString().Add("id", "179").ToString();
    /// </summary>
    /// <seealso cref="NameValueCollection" />
    [Serializable]
    public class QueryStringBuilder : NameValueCollection
    {

        #region ~Ctor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public QueryStringBuilder() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="queryString">The query string.</param>
        public QueryStringBuilder(string queryString)
        {
            FillFromString(queryString);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringBuilder"/> class.
        /// </summary>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the new <see cref="T:System.Collections.Specialized.NameValueCollection" /> instance.</param>
        /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains the source and destination of the serialized stream associated with the new <see cref="T:System.Collections.Specialized.NameValueCollection" /> instance.</param>
        protected QueryStringBuilder(SerializationInfo info, StreamingContext context) : base(info, context)
        { }

        #endregion

        #region Private methods

        /// <summary>
        /// extracts a querystring from a full URL.
        /// </summary>
        /// <param name="s">the string to extract the querystring from.</param>
        /// <returns>a string representing only the querystring.</returns>
        private static string ExtractQuerystring(string s)
        {
            if (string.IsNullOrWhiteSpace(s) || s.Contains(@"?"))
            {
                return s;
            }

            return s.Substring(s.IndexOf(@"?", StringComparison.Ordinal) + 1);
        }

        /// <summary>
        /// returns a querystring object based on a string.
        /// </summary>
        /// <param name="s">the string to parse.</param>
        /// <returns>the QueryString object.</returns>
        private void FillFromString(string s)
        {
            Clear();
            if (string.IsNullOrWhiteSpace(s))
            {
                return;
            }

            foreach (var split in from keyValuePair in ExtractQuerystring(s).Split('&') where !string.IsNullOrWhiteSpace(keyValuePair) select keyValuePair.Split('='))
            {
                base.Add(split[0], split.Length == 2 ? split[1] : "");
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// overrides the default.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>the associated decoded value for the specified name.</returns>
        public new string this[string name] => HttpUtility.UrlDecode(base[name]);

        /// <summary>
        /// overrides the default indexer.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>the associated decoded value for the specified index.</returns>
        public new string this[int index] => HttpUtility.UrlDecode(base[index]);

        #endregion

        #region Public methods

        /// <summary>
        /// adds a name value pair to the collection.
        /// </summary>
        /// <param name="name">the name.</param>
        /// <param name="value">the value associated to the name.</param>
        /// <param name="isUnique">true if the name is unique within the querystring. This allows us to override existing
        /// values.</param>
        /// <returns>the QueryString object.</returns>
        /// <exception cref="ArgumentNullException">.</exception>
        public QueryStringBuilder Add(string name, string value, bool isUnique = false)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var existingValue = base[name];
            var urlEncoded = HttpUtility.UrlEncode(value);
            if (string.IsNullOrWhiteSpace(existingValue) && !string.IsNullOrWhiteSpace(urlEncoded))
            {
                base.Add(name, urlEncoded);
            }
            else if (isUnique)
            {
                base[name] = urlEncoded;
            }
            else
            {
                base[name] += @"," + urlEncoded;
            }

            return this;
        }

        /// <summary>
        /// adds a name value pair to the collection.
        /// </summary>
        /// <param name="name">the name.</param>
        /// <param name="value">the value associated to the name.</param>
        /// <param name="isUnique">true if the name is unique within the querystring. This allows us to override existing
        /// values.</param>
        /// <returns>the QueryString object.</returns>
        public QueryStringBuilder Add(string name, object value, bool isUnique = false)
        {
            return Add(name, value.ToString(), isUnique);
        }

        /// <summary>
        /// Adds a range to 'isUnique'.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="isUnique">true if the name is unique within the querystring. This allows us to override existing
        /// values.</param>
        /// <returns>A QueryString.</returns>
        public QueryStringBuilder AddRange(Dictionary<string, string> items, bool isUnique = false)
        {
            foreach (var item in items)
            {
                Add(item.Key, item.Value, isUnique);
            }

            return this;
        }

        /// <summary>
        /// Adds from type to 'convertCamelCaseToUnderscore'.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="convertCamelCaseToUnderscore">(Optional) the convert camel case to underscore.</param>
        /// <returns>A QueryStringBuilder.</returns>
        public QueryStringBuilder AddFromType<T>(T instance, bool convertCamelCaseToUnderscore = true)
        {
            var type = typeof(T);
            var typeProperties = type.GetProperties();
            foreach (var property in typeProperties)
            {
                if (property.QueryStringBuilderIgnore())
                {
                    continue;
                }

                var propertyName = property.GetQueryStringBuilderKeyName() ?? property.Name;
                if (convertCamelCaseToUnderscore)
                {
                    propertyName = string.Concat(propertyName.Select((x, i) => i > 0 && char.IsUpper(x) ? @"_" + x.ToString(CultureInfo.InvariantCulture).ToLower() : x.ToString(CultureInfo.InvariantCulture)));
                }

                if (propertyName.StartsWith(type.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    propertyName = $@"{type.Name.ToLower()}[{propertyName.Substring(type.Name.Length + (convertCamelCaseToUnderscore ? 1 : 0))}]";
                }

                var value = property.GetValue(instance, null);
                Add(propertyName, value?.ToString() ?? string.Empty);
            }
            return this;
        }

        /// <summary>
        /// removes a name value pair from the querystring collection.
        /// </summary>
        /// <param name="name">name of the querystring value to remove.</param>
        /// <returns>the QueryString object.</returns>
        public new QueryStringBuilder Remove(string name)
        {
            var existingValue = base[name];
            if (!string.IsNullOrWhiteSpace(existingValue))
            {
                base.Remove(name);
            }

            return this;
        }

        /// <summary>
        /// clears the collection
        /// </summary>
        /// <returns>the QueryString object</returns>
        public QueryStringBuilder Reset()
        {
            Clear();
            return this;
        }

        /// <summary>
        /// checks if a name already exists within the query string collection.
        /// </summary>
        /// <param name="name">the name to check.</param>
        /// <returns>a boolean if the name exists.</returns>
        public bool Contains(string name)
        {
            var existingValue = base[name];
            return !string.IsNullOrWhiteSpace(existingValue);
        }

        /// <summary>
        /// outputs the querystring object to a string.
        /// </summary>
        /// <returns>the encoded querystring as it would appear in a browser.</returns>
        public override string ToString()
        {
            return ToStringInternal();
        }

        /// <summary>
        /// outputs the querystring object to a string.
        /// </summary>
        /// <param name="includeNullFields">true to include, false to exclude the null fields.</param>
        /// <returns>the encoded querystring as it would appear in a browser.</returns>
        public string ToString(bool includeNullFields)
        {
            return ToStringInternal(includeNullFields);
        }

        /// <summary>
        /// Converts the includeNullFields to a string internal.
        /// </summary>
        /// <param name="includeNullFields">true to include, false to exclude the null fields.</param>
        /// <returns>includeNullFields as a String.</returns>
        private string ToStringInternal(bool includeNullFields = false)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < Keys.Count; i++)
            {
                if (!includeNullFields && string.IsNullOrWhiteSpace(Keys[i]))
                {
                    continue;
                }

                foreach (var val in base[Keys[i]].Split(','))
                {
                    builder.Append(builder.Length == 0 ? @"?" : @"&").Append(HttpUtility.UrlEncode(Keys[i])).Append(@"=").Append(val);
                }
            }
            return builder.ToString();
        }

        #endregion
    }
}

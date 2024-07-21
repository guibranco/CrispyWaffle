using System;

namespace CrispyWaffle.Attributes
{
    /// <summary>
    /// The query string builder ignore attribute class.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class QueryStringBuilderIgnoreAttribute : Attribute { }
}

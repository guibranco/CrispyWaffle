using System;
using System.Diagnostics.CodeAnalysis;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Tests.Serialization.TestObjects_;

[Serializer(SerializerFormat.Json, false)]
[ExcludeFromCodeCoverage]
public class SampleJsonNotStrictClass : SampleJsonClass, IEquatable<SampleJsonNotStrictClass>
{
    public bool Equals(SampleJsonNotStrictClass other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return string.Equals(Text, other.Text, StringComparison.InvariantCultureIgnoreCase)
            && Equals(StrongTyping, other.StrongTyping);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((SampleJsonNotStrictClass)obj);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Text, StringComparer.InvariantCultureIgnoreCase);
        hashCode.Add(StrongTyping);
        return hashCode.ToHashCode();
    }

    public static bool operator ==(SampleJsonNotStrictClass left, SampleJsonNotStrictClass right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(SampleJsonNotStrictClass left, SampleJsonNotStrictClass right)
    {
        return !Equals(left, right);
    }

    public string Text { get; set; }

    public StrongTypingClass StrongTyping { get; set; }
}

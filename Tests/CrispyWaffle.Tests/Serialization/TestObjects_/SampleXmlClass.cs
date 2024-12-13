using System;
using System.Diagnostics.CodeAnalysis;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Tests.Serialization;

[Serializer]
[ExcludeFromCodeCoverage]
public sealed class SampleXmlClass : IEquatable<SampleXmlClass>
{
    public bool Equals(SampleXmlClass other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Code == other.Code
            && string.Equals(String, other.String, StringComparison.OrdinalIgnoreCase)
            && CorrelationId.Equals(other.CorrelationId)
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

        return Equals((SampleXmlClass)obj);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Code);
        hashCode.Add(String, StringComparer.InvariantCultureIgnoreCase);
        hashCode.Add(CorrelationId);
        hashCode.Add(StrongTyping);
        return hashCode.ToHashCode();
    }

    public static bool operator ==(SampleXmlClass left, SampleXmlClass right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(SampleXmlClass left, SampleXmlClass right)
    {
        return !Equals(left, right);
    }

    public int Code { get; set; }

    public string String { get; set; }

    public Guid CorrelationId { get; set; }

    public StrongTypingClass StrongTyping { get; set; }
}

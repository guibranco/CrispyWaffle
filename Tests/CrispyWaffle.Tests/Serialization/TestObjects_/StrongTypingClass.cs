using System;
using System.Diagnostics.CodeAnalysis;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Tests.Serialization.TestObjects_;

[Serializer(SerializerFormat.Json)]
[ExcludeFromCodeCoverage]
public class StrongTypingClass : IEquatable<StrongTypingClass>
{
    public bool Equals(StrongTypingClass other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return CorrelationId.Equals(other.CorrelationId)
            && string.Equals(SomeText, other.SomeText, StringComparison.OrdinalIgnoreCase)
            && Date.Equals(other.Date);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((StrongTypingClass)obj);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(CorrelationId);
        hashCode.Add(SomeText, StringComparer.InvariantCultureIgnoreCase);
        hashCode.Add(Date);
        return hashCode.ToHashCode();
    }

    public static bool operator ==(StrongTypingClass left, StrongTypingClass right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(StrongTypingClass left, StrongTypingClass right)
    {
        return !Equals(left, right);
    }

    public Guid CorrelationId { get; set; }

    public string SomeText { get; set; }

    public DateTime Date { get; set; }
}

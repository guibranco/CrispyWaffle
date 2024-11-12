using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Tests.Serialization;

[Serializer(SerializerFormat.Json)]
[ExcludeFromCodeCoverage]
public class SampleJsonClass : IEquatable<SampleJsonClass>
{
    public bool Equals(SampleJsonClass other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Id.Equals(other.Id)
            && Date.Equals(other.Date)
            && ListStrong.SequenceEqual(other.ListStrong);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((SampleJsonClass)obj);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode() => HashCode.Combine(Id, Date, ListStrong);

    public static bool operator ==(SampleJsonClass left, SampleJsonClass right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(SampleJsonClass left, SampleJsonClass right)
    {
        return !Equals(left, right);
    }

    public Guid Id { get; set; }

    public DateTime Date { get; set; }

    public List<StrongTypingClass> ListStrong { get; set; }
}

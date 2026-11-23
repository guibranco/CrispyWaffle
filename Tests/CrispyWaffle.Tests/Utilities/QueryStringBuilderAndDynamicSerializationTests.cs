using System.Collections.Generic;
using System.IO;
using System.Xml;
using CrispyWaffle.Utilities;
using Xunit;

namespace CrispyWaffle.Tests.Utilities;

public sealed class QueryStringBuilderAndDynamicSerializationTests
{
    [Fact]
    public void QueryStringBuilderAddEmptyValueDoesNotDuplicateKey()
    {
        var singleEmpty = new QueryStringBuilder().Add("a", string.Empty).ToString();
        var emptyThenValue = new QueryStringBuilder().Add("a", string.Empty).Add("a", "1").ToString();

        Assert.Equal("?a=", singleEmpty);
        Assert.Equal("?a=&a=1", emptyThenValue);
    }

    [Fact]
    public void QueryStringBuilderAddFromTypeWithNullOrEmptyPropertyDoesNotDuplicateKey()
    {
        var sample = new SampleQueryModel { EmptyProp = string.Empty, NullProp = null, ValueProp = "ok" };
        var queryString = new QueryStringBuilder().AddFromType(sample).ToString();

        Assert.Equal("?Empty_prop=&Null_prop=&Value_prop=ok", queryString);
    }

    [Fact]
    public void DynamicSerializationCamelcaseLowercasesLeadingCharacterOnly()
    {
        var dynamicSerialization = new DynamicSerialization(DynamicSerializationOption.Camelcase);
        dynamicSerialization.SetMember("userName", "x");
        dynamicSerialization.SetMember("OrderId", 1);

        using var stringWriter = new StringWriter();
        using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment }))
        {
            dynamicSerialization.WriteXml(xmlWriter);
        }

        Assert.Equal("<userName>x</userName><orderId>1</orderId>", stringWriter.ToString());
    }

    [Fact]
    public void DynamicSerializationGetHashCodeConsistentWithEquals()
    {
        var first = new DynamicSerialization(DynamicSerializationOption.Camelcase);
        first.SetMember("a", 1);
        first.SetMember("b", "two");

        var second = new DynamicSerialization(DynamicSerializationOption.Camelcase);
        second.SetMember("a", 1);
        second.SetMember("b", "two");

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());

        var set = new HashSet<DynamicSerialization> { first };
        Assert.Contains(second, set);
    }

    private sealed class SampleQueryModel
    {
        public string EmptyProp { get; set; }

        public string NullProp { get; set; }

        public string ValueProp { get; set; }
    }
}

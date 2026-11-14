using System.IO;
using System.Text;
using System.Xml;
using CrispyWaffle.Extensions;
using Xunit;

namespace CrispyWaffle.Tests.Extensions;

public class XmlExtensionsTests
{
    [Fact]
    public void DuplicateReadersReturnsIndependentReaders()
    {
        var xml = new StringBuilder("<root>");
        for (var i = 0; i < 2000; i++)
        {
            xml.Append("<i>").Append(i).Append("</i>");
        }

        xml.Append("</root>");

        XmlReader
            .Create(new StringReader(xml.ToString()))
            .DuplicateReaders(out var cloneOne, out var cloneTwo);

        var documentOne = new XmlDocument();
        documentOne.Load(cloneOne);
        var documentTwo = new XmlDocument();
        documentTwo.Load(cloneTwo);

        Assert.Equal(2000, documentOne.DocumentElement!.ChildNodes.Count);
        Assert.Equal(2000, documentTwo.DocumentElement!.ChildNodes.Count);
    }
}

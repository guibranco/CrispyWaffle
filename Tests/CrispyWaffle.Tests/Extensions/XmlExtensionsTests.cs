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

    [Fact]
    public void DuplicateReadersCanBeReadInterleaved()
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

        using (cloneOne)
        using (cloneTwo)
        {
            var countOne = 0;
            var countTwo = 0;
            var moreOne = true;
            var moreTwo = true;

            while (moreOne || moreTwo)
            {
                if (moreOne && (moreOne = cloneOne.Read()) && cloneOne.Name == "i")
                {
                    countOne++;
                }

                if (moreTwo && (moreTwo = cloneTwo.Read()) && cloneTwo.Name == "i")
                {
                    countTwo++;
                }
            }

            // Each <i> element has a start and an end tag.
            Assert.Equal(4000, countOne);
            Assert.Equal(4000, countTwo);
        }
    }
}

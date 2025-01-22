using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace CrispyWaffle.Extensions;

/// <summary>
/// Provides extension methods for working with XML data in <see cref="XmlReader"/> and <see cref="XmlDocument"/>.
/// These methods simplify common XML operations like duplicating readers, fetching node values, and retrieving elements by tag name.
/// </summary>
public static class XmlExtensions
{
    /// <summary>
    /// Duplicates an <see cref="XmlReader"/> into two independent readers.
    /// This is useful when you need to read the same XML content multiple times or in different ways.
    /// </summary>
    /// <param name="xmlReader">The <see cref="XmlReader"/> to be duplicated. This will be closed after duplication.</param>
    /// <param name="cloneOne">The first <see cref="XmlReader"/> clone, created from the original reader.</param>
    /// <param name="cloneTwo">The second <see cref="XmlReader"/> clone, created from the original reader.</param>
    /// <remarks>
    /// The method creates an <see cref="XmlDocument"/> to load the XML content from the original reader.
    /// It then saves the document to a <see cref="MemoryStream"/> to allow the creation of two separate clones.
    /// </remarks>
    public static void DuplicateReaders(
        this XmlReader xmlReader,
        out XmlReader cloneOne,
        out XmlReader cloneTwo
    )
    {
        var document = new XmlDocument();
        var stream = new MemoryStream();
        document.Load(xmlReader);
        document.Save(stream);
        var settings = new XmlReaderSettings { IgnoreWhitespace = true };
        stream.Position = 0;
        cloneOne = XmlReader.Create(stream, settings);
        stream.Position = 0;
        cloneTwo = XmlReader.Create(stream, settings);
        xmlReader.Close();
    }

    /// <summary>
    /// Retrieves the first element in the XML document with the specified tag name.
    /// </summary>
    /// <param name="document">The <see cref="XmlDocument"/> containing the XML data.</param>
    /// <param name="tagName">The name of the tag to search for.</param>
    /// <returns>
    /// The first <see cref="XmlNode"/> matching the tag name, or <c>null</c> if no matching elements are found.
    /// </returns>
    /// <remarks>
    /// This method uses <see cref="XmlDocument.GetElementsByTagName"/> to get all elements with the specified tag
    /// and returns the first element in the collection, if any.
    /// </remarks>
    public static XmlNode GetFirstElementByTagName(this XmlDocument document, string tagName)
    {
        var elements = document.GetElementsByTagName(tagName);
        return elements.Count >= 1 ? elements.Item(0) : null;
    }

    /// <summary>
    /// Retrieves the value of a specific child node from an XML node.
    /// </summary>
    /// <param name="xmlNode">The <see cref="XmlNode"/> containing the child node.</param>
    /// <param name="node">The name of the child node whose value is to be retrieved.</param>
    /// <returns>
    /// The inner text of the child node if found; otherwise, an empty string.
    /// </returns>
    /// <remarks>
    /// This method uses <see cref="XmlNode.SelectNodes"/> to select the child node by its name and fetches the
    /// <see cref="XmlNode.InnerText"/> if the node is found.
    /// </remarks>
    public static string GetNodeValue(this XmlNode xmlNode, string node)
    {
        var nodes = xmlNode.SelectNodes(node);
        var item = nodes?.Item(0);
        return item != null ? item.InnerText : string.Empty;
    }

    /// <summary>
    /// Retrieves the values of multiple child nodes from an XML node.
    /// </summary>
    /// <param name="xmlNode">The <see cref="XmlNode"/> containing the child nodes.</param>
    /// <param name="nodes">An array of node names whose values are to be retrieved.</param>
    /// <returns>
    /// A string containing the concatenated values of the requested child nodes, separated by a space.
    /// If a node is not found, an empty string is used in its place.
    /// </returns>
    /// <remarks>
    /// This method uses <see cref="XmlNode.SelectNodes"/> for each node name in the <paramref name="nodes"/>
    /// array, collects the values of the found nodes, and concatenates them into a single string with a space separator.
    /// </remarks>
    public static string GetNodeValue(this XmlNode xmlNode, string[] nodes)
    {
        var results = new List<string>(nodes.Length);
        foreach (var node in nodes)
        {
            var nodesL = xmlNode.SelectNodes(node);
            if (nodesL == null || nodesL.Count < 1)
            {
                results.Add(string.Empty);
                continue;
            }

            var item = nodesL.Item(0);
            if (item != null)
            {
                results.Add(item.InnerText);
            }
        }

        return string.Join(" ", results);
    }
}

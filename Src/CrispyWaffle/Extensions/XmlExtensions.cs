namespace CrispyWaffle.Extensions
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// The XML extensions class.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Duplicates the readers.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="cloneOne">The clone one.</param>
        /// <param name="cloneTwo">The clone two.</param>
        public static void DuplicateReaders(this XmlReader xmlReader, out XmlReader cloneOne, out XmlReader cloneTwo)
        {
            var document = new XmlDocument();
            var stream = new MemoryStream();
            document.Load(xmlReader);
            document.Save(stream);
            var settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true
            };
            stream.Position = 0;
            cloneOne = XmlReader.Create(stream, settings);
            stream.Position = 0;
            cloneTwo = XmlReader.Create(stream, settings);
            xmlReader.Close();
        }

        /// <summary>
        /// Gets the first name of the element by tag.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns></returns>
        public static XmlNode GetFirstElementByTagName(this XmlDocument document, string tagName)
        {
            var elements = document.GetElementsByTagName(tagName);
            return elements.Count >= 1
                       ? elements.Item(0)
                       : null;
        }

        /// <summary>
        /// Gets the node value.
        /// </summary>
        /// <param name="xmlNode">The XML node.</param>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static string GetNodeValue(this XmlNode xmlNode, string node)
        {
            var nodes = xmlNode.SelectNodes(node);
            var item = nodes?.Item(0);
            if (item == null)
            {
                return string.Empty;
            }

            return nodes.Count >= 1
                ? item.InnerText
                : string.Empty;
        }

        /// <summary>
        /// Gets the node value.
        /// </summary>
        /// <param name="xmlNode">The XML node.</param>
        /// <param name="nodes">The nodes.</param>
        /// <returns></returns>
        public static string GetNodeValue(this XmlNode xmlNode, string[] nodes)
        {
            var results = new List<string>(nodes.Length);
            foreach (var node in nodes)
            {
                var nodesL = xmlNode.SelectNodes(node);
                if (nodesL == null ||
                    nodesL.Count < 1)
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
            return string.Join<string>(@" ", results);
        }
    }
}

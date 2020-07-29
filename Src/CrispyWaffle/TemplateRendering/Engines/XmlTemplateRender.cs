namespace CrispyWaffle.TemplateRendering.Engines
{
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Xsl;

    /// <summary>
    /// The xml template render class.
    /// </summary>
    /// <seealso cref="ITemplateRender" />
    public sealed class XmlTemplateRender : ITemplateRender
    {
        #region Implementation of ITemplateRender

        /// <summary>
        /// The render method, that receives the template and the data to process.
        /// </summary>
        /// <param name="template">The HTML template</param>
        /// <param name="data">The data to resolve in template</param>
        /// <returns>The template resolved</returns>
        public string Render(string template, object data)
        {
            var document = (XNode)data;
            var xsl = new XslCompiledTransform();
            using (var stringReader = new StringReader(template))
            {
                xsl.Load(XmlReader.Create(stringReader));
            }

            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
            {
                xsl.Transform(document.CreateReader(), null, new XmlTextWriter(stringWriter));
            }

            return stringBuilder.ToString();
        }

        #endregion
    }
}

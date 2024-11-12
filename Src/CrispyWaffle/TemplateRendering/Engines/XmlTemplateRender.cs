using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace CrispyWaffle.TemplateRendering.Engines;

/// <summary>
/// A template renderer that processes XML templates using XSL transformations.
/// </summary>
/// <seealso cref="ITemplateRender" />
public sealed class XmlTemplateRender : ITemplateRender
{
    /// <summary>
    /// Renders the provided XML template by applying the specified data through XSL transformation.
    /// </summary>
    /// <param name="template">
    /// The XSLT template to apply. This template should be a valid XSLT document that defines how
    /// the data will be transformed into the desired output.
    /// </param>
    /// <param name="data">
    /// The data to resolve in the template. The data should be in the form of an <see cref="XNode"/>
    /// (typically an <see cref="XDocument"/> or <see cref="XElement"/> instance), which will be processed
    /// by the XSL transformation.
    /// </param>
    /// <returns>
    /// A string representing the result of applying the XSLT template to the provided data.
    /// The transformed output is returned as an XML string.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either <paramref name="template"/> or <paramref name="data"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if an error occurs during the XSL transformation process.
    /// </exception>
    public string Render(string template, object data)
    {
        if (template == null)
            throw new ArgumentNullException(nameof(template));
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var document = (XNode)data;
        var xsl = new XslCompiledTransform();

        // Load the XSLT template from the provided string.
        using (var stringReader = new StringReader(template))
        {
            xsl.Load(XmlReader.Create(stringReader));
        }

        // Prepare for the transformation and write the result to a string.
        var stringBuilder = new StringBuilder();
        using (var stringWriter = new StringWriter(stringBuilder))
        {
            xsl.Transform(document.CreateReader(), null, new XmlTextWriter(stringWriter));
        }

        return stringBuilder.ToString();
    }
}

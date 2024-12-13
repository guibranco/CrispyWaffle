namespace CrispyWaffle.TemplateRendering.Engines;

/// <summary>
/// Defines the contract for a template rendering engine.
/// </summary>
/// <remarks>
/// Implementations of this interface should provide logic to process templates with provided data
/// and return the rendered result as a string.
/// </remarks>
public interface ITemplateRender
{
    /// <summary>
    /// Renders the provided template by resolving placeholders with the given data.
    /// </summary>
    /// <param name="template">The HTML template that contains placeholders to be replaced with data.</param>
    /// <param name="data">The data object used to resolve the placeholders within the template.</param>
    /// <returns>A string representing the rendered template with data inserted.</returns>
    /// <remarks>
    /// This method processes the template by replacing placeholders with corresponding values
    /// from the provided data object. The exact behavior of the rendering process may vary depending
    /// on the implementation of the <see cref="ITemplateRender"/> interface.
    /// </remarks>
    string Render(string template, object data);
}

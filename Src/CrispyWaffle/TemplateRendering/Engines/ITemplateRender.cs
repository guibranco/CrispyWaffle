namespace CrispyWaffle.TemplateRendering.Engines
{
    /// <summary>
    /// An interface for template render engine.
    /// </summary>

    public interface ITemplateRender
    {
        /// <summary>
        /// The render method, that receives the template and the data to process.
        /// </summary>
        /// <param name="template">The HTML template</param>
        /// <param name="data">The data to resolve in template</param>
        /// <returns>The template resolved</returns>
        string Render(string template, object data);
    }
}

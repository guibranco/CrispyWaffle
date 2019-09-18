namespace CrispyWaffle.TemplateRendering.Repositories
{
    /// <summary>
    /// Template repository interface
    /// </summary>

    public interface ITemplateRepository
    {
        /// <summary>
        /// Register a template using a unique identifier
        /// </summary>
        /// <param name="name">The template name (unique identifier)</param>
        /// <param name="content">The template itself</param>
        void RegisterTemplate(string name, string content);

        /// <summary>
        /// Get a template by it's name.
        /// </summary>
        /// <param name="name">The name of the template</param>
        /// <returns>The template as string or null if no template is found by name/identifier supplied</returns>
        string GetTemplateByName(string name);
    }
}

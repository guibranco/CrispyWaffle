namespace CrispyWaffle.TemplateRendering.Repositories
{
    /// <summary>
    /// Defines methods for managing templates within a repository.
    /// </summary>
    /// <remarks>
    /// This interface provides operations to register and retrieve templates by a unique identifier (name).
    /// Implementations of this interface may store templates in memory, a database, or any other storage mechanism.
    /// </remarks>
    public interface ITemplateRepository
    {
        /// <summary>
        /// Registers a new template with a unique name.
        /// </summary>
        /// <param name="name">The unique name (identifier) for the template.</param>
        /// <param name="content">The content of the template as a string.</param>
        /// <remarks>
        /// If a template with the same name already exists, the behavior depends on the implementation (e.g., it could overwrite the existing template or throw an exception).
        /// </remarks>
        void RegisterTemplate(string name, string content);

        /// <summary>
        /// Retrieves a template by its unique name.
        /// </summary>
        /// <param name="name">The name (identifier) of the template to retrieve.</param>
        /// <returns>
        /// A string containing the template content, or <c>null</c> if no template with the specified name is found.
        /// </returns>
        /// <remarks>
        /// This method returns <c>null</c> if the template is not found. Ensure the template name provided is correct.
        /// </remarks>
        string GetTemplateByName(string name);
    }
}

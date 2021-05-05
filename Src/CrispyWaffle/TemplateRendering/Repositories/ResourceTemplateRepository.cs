namespace CrispyWaffle.TemplateRendering.Repositories
{
    using System;
    using System.Globalization;
    using System.Resources;

    /// <summary>
    /// The resource template repository class.
    /// This class provides a template repository based on application resources
    /// passed as reference to the constructor.
    /// </summary>

    public sealed class ResourceTemplateRepository : ITemplateRepository
    {
        #region Private Members

        /// <summary>
        /// The resource manager, source of the content.
        /// </summary>
        private readonly ResourceManager _manager;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Default constructor. 
        /// </summary>
        /// <param name="manager"><see cref="ResourceManager"/></param>
        public ResourceTemplateRepository(ResourceManager manager)
        {
            _manager = manager;
        }

        #endregion

        #region Implementation of ITemplateRepository

        /// <summary>
        /// Register a template using a unique identifier
        /// </summary>
        /// <param name="name">The template name (unique identifier)</param>
        /// <param name="content">The template itself</param>
        public void RegisterTemplate(string name, string content)
        {
            throw new InvalidOperationException("The registration of resource at runtime is not allowed.");
        }

        /// <summary>
        /// Get a template by it's name.
        /// </summary>
        /// <param name="name">The name of the template</param>
        /// <returns>The template as string or null if no template is found by name/identifier supplied</returns>
        public string GetTemplateByName(string name)
        {
            var result = _manager.GetString(name, CultureInfo.CurrentCulture);
            if (result == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to find the template {0} in the repository {1}", name, GetType().FullName));
            }

            return result;
        }

        #endregion
    }
}

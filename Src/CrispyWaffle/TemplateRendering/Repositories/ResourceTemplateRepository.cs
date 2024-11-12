using System;
using System.Globalization;
using System.Resources;

namespace CrispyWaffle.TemplateRendering.Repositories;

/// <summary>
/// A template repository that retrieves templates from application resources.
/// </summary>
/// <remarks>
/// This class allows templates to be stored as resources within the application and retrieved at runtime.
/// The <see cref="ResourceManager"/> passed to the constructor is used to access the templates based on their unique names.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="ResourceTemplateRepository"/> class.
/// </remarks>
/// <param name="manager">The <see cref="ResourceManager"/> that will be used to retrieve templates from resources.</param>
/// <remarks>
/// The <see cref="ResourceManager"/> should reference the resource file containing the templates.
/// </remarks>
public sealed class ResourceTemplateRepository(ResourceManager manager) : ITemplateRepository
{
    /// <summary>
    /// Throws an exception as registering templates at runtime is not supported in this repository.
    /// </summary>
    /// <param name="name">The name (unique identifier) for the template.</param>
    /// <param name="content">The content of the template to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Always thrown since registering templates at runtime is not allowed in this repository.
    /// </exception>
    public void RegisterTemplate(string name, string content)
    {
        throw new InvalidOperationException(
            "The registration of resource at runtime is not allowed."
        );
    }

    /// <summary>
    /// Retrieves a template from the repository using its unique name.
    /// </summary>
    /// <param name="name">The unique name (identifier) of the template to retrieve.</param>
    /// <returns>
    /// A string containing the template content, or <c>null</c> if no template is found by the given name.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the template with the specified name cannot be found in the resources.
    /// </exception>
    /// <remarks>
    /// The template is retrieved using the <see cref="ResourceManager"/> and the current culture.
    /// If the template is not found, an <see cref="InvalidOperationException"/> is thrown.
    /// </remarks>
    public string GetTemplateByName(string name)
    {
        var result = manager.GetString(name, CultureInfo.CurrentCulture);
        if (result == null)
        {
            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "Unable to find the template '{0}' in the repository '{1}'.",
                    name,
                    GetType().FullName
                )
            );
        }

        return result;
    }
}

using System.Collections.Generic;

namespace CrispyWaffle.Utilities;

/// <summary>
/// The error response class.
/// </summary>
public class ErrorResponse : IJsonResponse
{
    /// <summary>
    /// Gets or sets the error list.
    /// </summary>
    /// <value>The error list.</value>
    public Dictionary<string, IEnumerable<string>> ErrorList { get; set; }

    /// <summary>
    /// Gets the code.
    /// </summary>
    /// <value>The code.</value>
    public int Code { get; set; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    /// <value>The error message.</value>
    public string ErrorMessage { get; set; }
}

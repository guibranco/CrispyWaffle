using System;
using System.Runtime.Serialization;

namespace CrispyWaffle.Serialization.Adapters;

/// <summary>
/// Represents an error that occurs when a local file cannot be found in a specified directory.
/// </summary>
/// <remarks>
/// This exception is thrown when an operation tries to access a file that does not exist at the specified location.
/// The exception message provides details about the file and its expected directory path.
/// </remarks>
[Serializable]
public class LocalFileNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalFileNotFoundException"/> class.
    /// </summary>
    /// <param name="file">The name of the file that could not be found.</param>
    /// <param name="fullPath">The full directory path where the file was expected to be located.</param>
    /// <remarks>
    /// This constructor sets the exception message to indicate that the specified file could not be found in the given directory.
    /// </remarks>
    public LocalFileNotFoundException(string file, string fullPath)
        : base($"Unable to find the file {file} in the directory {fullPath}") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalFileNotFoundException"/> class with serialized data.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> object that holds the serialized object data.</param>
    /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
    /// <remarks>
    /// This constructor is used during deserialization of the exception.
    /// </remarks>
    protected LocalFileNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

namespace CrispyWaffle.Serialization.Adapters
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The local file not found exception class.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class LocalFileNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileNotFoundException" /> class.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="fullPath">The full path.</param>
        public LocalFileNotFoundException(string file, string fullPath)
            : base($"Unable to find the file {file} in the directory {fullPath}")
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected LocalFileNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}

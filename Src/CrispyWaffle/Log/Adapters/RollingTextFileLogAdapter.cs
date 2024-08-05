using System;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Log.Adapters
{
    /// <summary>
    /// Class RollingTextFileLogAdapter. This class cannot be inherited.
    /// Implements the <see cref="ITextFileLogAdapter" />.
    /// </summary>
    /// <seealso cref="ITextFileLogAdapter" />
    public sealed class RollingTextFileLogAdapter : ITextFileLogAdapter
    {
        /// <inheritdoc />
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SetLevel(LogLevel level)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Debug<T>(
            T content,
            string identifier,
            SerializerFormat customFormat = SerializerFormat.None
        )
            where T : class
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Debug(string content, string fileName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Debug(string message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Trace(Exception exception)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Trace(string message, Exception exception)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Trace(string message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Info(string message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Warning(string message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Error(string message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Fatal(string message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CategorizedFatal(string category, string message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CategorizedError(string category, string message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CategorizedWarning(string category, string message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CategorizedInfo(string category, string message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CategorizedTrace(string category, string message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CategorizedTrace(string category, string message, Exception exception)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CategorizedTrace(string category, Exception exception)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CategorizedDebug(string category, string message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CategorizedDebug(string category, string content, string identifier)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CategorizedDebug<T>(
            string category,
            T content,
            string identifier,
            SerializerFormat customFormat = SerializerFormat.None
        )
            where T : class
        {
            throw new NotImplementedException();
        }
    }
}

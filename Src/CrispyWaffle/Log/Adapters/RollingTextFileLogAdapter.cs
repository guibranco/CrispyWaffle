namespace CrispyWaffle.Log.Adapters
{
    using Serialization;
    using System;

    public sealed class RollingTextFileLogAdapter : ITextFileLogAdapter
    {
        #region Private fields

        #endregion

        #region ~Ctors

        #endregion

        #region Private methods

        #endregion

        #region Implementation of IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of ILogAdapter

        /// <inheritdoc />
        public void SetLevel(LogLevel level)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Debug<T>(T content, string identifier, SerializerFormat customFormat = SerializerFormat.NONE) where T : class
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

        #endregion

        #region Implementation of ICategorizedLogAdapter

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
        public void CategorizedDebug<T>(string category, T content, string identifier,
            SerializerFormat customFormat = SerializerFormat.NONE) where T : class
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

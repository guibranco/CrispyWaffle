// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 07-29-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="TestLogProvider.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.Tests
{
    using CrispyWaffle.Extensions;
    using Log;
    using Log.Providers;
    using CrispyWaffle.Serialization;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Xunit.Abstractions;

    /// <summary>
    /// Class TestLogProvider.
    /// Implements the <see cref="CrispyWaffle.Log.Providers.ILogProvider" />
    /// </summary>
    /// <seealso cref="CrispyWaffle.Log.Providers.ILogProvider" />
    [ExcludeFromCodeCoverage]
    internal class TestLogProvider : ILogProvider
    {

        /// <summary>
        /// The test output helper
        /// </summary>
        private readonly ITestOutputHelper _testOutputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestLogProvider"/> class.
        /// </summary>
        /// <param name="testOutputHelper">The test output helper.</param>
        /// <exception cref="ArgumentNullException">_testOutputHelper</exception>
        public TestLogProvider(ITestOutputHelper testOutputHelper) => _testOutputHelper =
            testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));


        #region Implementation of ILogProvider

        /// <summary>
        /// Sets the log level of the instance
        /// </summary>
        /// <param name="level">The log level</param>
        public void SetLevel(LogLevel level)
        {
            _testOutputHelper.WriteLine("Set level: {0}", level.GetHumanReadableValue());
        }

        /// <summary>
        /// Logs the message with fatal level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        public void Fatal(string category, string message)
        {
            _testOutputHelper.WriteLine("Fatal: {0} - {1}", category, message);
        }

        /// <summary>
        /// Logs the message with error level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        public void Error(string category, string message)
        {
            _testOutputHelper.WriteLine("Error: {0} - {1}", category, message);
        }

        /// <summary>
        /// Logs the message with warning level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged</param>
        public void Warning(string category, string message)
        {
            _testOutputHelper.WriteLine("Warning: {0} - {1}", category, message);
        }

        /// <summary>
        /// Logs the message with info level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged</param>
        public void Info(string category, string message)
        {
            //_testOutputHelper.WriteLine("Info: {0} - {1}", category, message);
            System.Diagnostics.Trace.WriteLine($"Info: {category} - {message}");
        }

        /// <summary>
        /// Logs the message with trace level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged</param>
        public void Trace(string category, string message)
        {
            //_testOutputHelper.WriteLine("Trace: {0} - {1}", category, message);
            System.Diagnostics.Trace.WriteLine($"Trace: {category} - {message}");
        }

        /// <summary>
        /// Logs the message with trace level and shows exception details.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception.</param>
        public void Trace(string category, string message, Exception exception)
        {
            _testOutputHelper.WriteLine("Trace: {0} - {1}", category, message);
            WriteExceptionDetails("Trace", category, exception);
        }

        /// <summary>
        /// Logs the exception details with trace level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="exception">The exception.</param>
        public void Trace(string category, Exception exception)
        {
            WriteExceptionDetails("Trace", category, exception);
        }

        /// <summary>
        /// Logs the message with debug level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged</param>
        public void Debug(string category, string message)
        {
            _testOutputHelper.WriteLine("Debug: {0} - {1}", category, message);
        }

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier with debug level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="content">The content to be stored</param>
        /// <param name="identifier">The file name of the content. This can be a filename, a key, a identifier. Depends upon each implementation</param>
        public void Debug(string category, string content, string identifier)
        {
            _testOutputHelper.WriteLine("Error: {0} - {1}", category, identifier);
            _testOutputHelper.WriteLine(content);
        }

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier with debug level using a custom serializer or default.
        /// </summary>
        /// <typeparam name="T">any class that can be serialized to the <paramref name="customFormat" /> serializer format</typeparam>
        /// <param name="category">The category.</param>
        /// <param name="content">The object to be serialized</param>
        /// <param name="identifier">The filename/attachment identifier (file name or key)</param>
        /// <param name="customFormat">(Optional) the custom serializer format</param>
        public void Debug<T>(string category, T content, string identifier, SerializerFormat customFormat) where T : class, new()
        {
            _testOutputHelper.WriteLine("Error: {0} - {1}", category, identifier);
            _testOutputHelper.WriteLine((string)content.GetCustomSerializer(customFormat));
        }

        #endregion

        /// <summary>
        /// Writes the exception details.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="category">The category.</param>
        /// <param name="exception">The exception.</param>
        private void WriteExceptionDetails(string level, string category, Exception exception)
        {
            do
            {
                _testOutputHelper.WriteLine("{0}: {1} - {2}", level, category, exception.Message);
                _testOutputHelper.WriteLine("{0}: {1} - {2}", level, category, exception.GetType().FullName);
                _testOutputHelper.WriteLine("{0}: {1} - {2}", level, category, exception.StackTrace);

                exception = exception.InnerException;
            } while (exception != null);
        }
    }
}

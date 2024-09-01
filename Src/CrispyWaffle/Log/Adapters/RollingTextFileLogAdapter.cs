﻿using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using CrispyWaffle.Attributes;
using CrispyWaffle.Extensions;
using CrispyWaffle.Infrastructure;
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
        private const long MinFileSizeAllowed = 1L * 1024;
        private const long MaxFileSizeAllowed = 10L * 1024 * 1024 * 1024;
        private readonly int _maxMessageCount;
        private readonly string _folderPath;
        private readonly string _fileNameSeed;
        private readonly string _defaultCategory;
        private readonly long _maxFileSize;
        private readonly object _syncRoot;
        private readonly LogFileType _fileType;
        private int fileNumber;
        private int fileMessageCount;
        private string currentFileName;
        private LogLevel _level;
        private FileStream currentFileStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollingTextFileLogAdapter"/> class.
        /// </summary>
        /// <param name="folderPath">The path to directory where log files are to be saved.</param>
        /// <param name="fileNameSeed">An identifier to be included in every file generated by this object.</param>
        /// <param name="maxMessageCount">The max count of messages allowed in a specific log file, after which a new log file is created.</param>
        /// <param name="maxFileSize">Max file size after which a new log file is created.</param>
        /// <param name="fileType">The type of file to create for logs.</param>
        public RollingTextFileLogAdapter(
            string folderPath,
            string fileNameSeed,
            int maxMessageCount,
            (Unit unit, uint size) maxFileSize,
            LogFileType fileType = LogFileType.Text
        )
        {
            try
            {
                checked
                {
                    _maxFileSize =
                        maxFileSize.size * (long)maxFileSize.unit > MaxFileSizeAllowed
                        || maxFileSize.size * (long)maxFileSize.unit < MinFileSizeAllowed
                            ? throw new ArgumentOutOfRangeException(
                                $"File size cannot be greater than {MaxFileSizeAllowed} bytes and less than {MinFileSizeAllowed} bytes!"
                            )
                            : maxFileSize.size * (long)maxFileSize.unit;
                }
            }
            catch
            {
                throw new ArgumentOutOfRangeException(
                    $"File size cannot be greater than {MaxFileSizeAllowed} bytes and less than {MinFileSizeAllowed} bytes!"
                );
            }

            if (maxMessageCount < 1)
            {
                throw new ArgumentOutOfRangeException(
                    $"Max message count cannot be less than 1. Current value: ${maxMessageCount}!"
                );
            }

            _maxMessageCount = maxMessageCount;
            fileNumber = 1;
            _fileNameSeed = fileNameSeed;
            _level = LogLevel.Production;
            _defaultCategory = "N.A.";
            _fileType = fileType;
            _folderPath = Path.GetFullPath(folderPath);

            if (!Directory.Exists(_folderPath))
            {
                throw new DirectoryNotFoundException("The directory path provided is not correct!");
            }

            CreateNewFileName();
            currentFileStream = GetFile();
            _syncRoot = new object();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="RollingTextFileLogAdapter"/> class.
        /// </summary>
        ~RollingTextFileLogAdapter()
        {
            Dispose();
        }

        /// <inheritdoc />
        public void SetLevel(LogLevel level)
        {
            _level = level;
            Warning(
                $"Level updated from {_level.GetHumanReadableValue()} to {level.GetHumanReadableValue()}."
            );
        }

        /// <inheritdoc />
        public void Debug<T>(
            T content,
            string identifier,
            SerializerFormat customFormat = SerializerFormat.None
        )
            where T : class
        {
            if (customFormat == SerializerFormat.None)
            {
                WriteToFile(LogLevel.Debug, (string)content.GetSerializer(), fileName: identifier);

                return;
            }

            WriteToFile(
                LogLevel.Debug,
                (string)content.GetCustomSerializer(customFormat),
                fileName: identifier
            );
        }

        /// <inheritdoc />
        public void Debug(string content, string fileName)
        {
            WriteToFile(LogLevel.Debug, content, fileName);
        }

        /// <inheritdoc />
        public void Debug(string message)
        {
            WriteToFile(LogLevel.Debug, message);
        }

        /// <inheritdoc />
        public void Trace(Exception exception)
        {
            WriteToFile(LogLevel.Trace, exception);
        }

        /// <inheritdoc />
        public void Trace(string message, Exception exception)
        {
            WriteToFile(LogLevel.Trace, message);
            WriteToFile(LogLevel.Trace, exception);
        }

        /// <inheritdoc />
        public void Trace(string message)
        {
            WriteToFile(LogLevel.Trace, message);
        }

        /// <inheritdoc />
        public void Info(string message)
        {
            WriteToFile(LogLevel.Info, message);
        }

        /// <inheritdoc />
        public void Warning(string message)
        {
            WriteToFile(LogLevel.Warning, message);
        }

        /// <inheritdoc />
        public void Error(string message)
        {
            WriteToFile(LogLevel.Error, message);
        }

        /// <inheritdoc />
        public void Fatal(string message)
        {
            WriteToFile(LogLevel.Fatal, message);
        }

        /// <inheritdoc />
        public void CategorizedFatal(string category, string message)
        {
            WriteToFile(LogLevel.Fatal, message, category: category);
        }

        /// <inheritdoc />
        public void CategorizedError(string category, string message)
        {
            WriteToFile(LogLevel.Error, message, category: category);
        }

        /// <inheritdoc />
        public void CategorizedWarning(string category, string message)
        {
            WriteToFile(LogLevel.Warning, message, category: category);
        }

        /// <inheritdoc />
        public void CategorizedInfo(string category, string message)
        {
            WriteToFile(LogLevel.Info, message, category: category);
        }

        /// <inheritdoc />
        public void CategorizedTrace(string category, string message)
        {
            WriteToFile(LogLevel.Trace, message, category: category);
        }

        /// <inheritdoc />
        public void CategorizedTrace(string category, string message, Exception exception)
        {
            WriteToFile(LogLevel.Trace, message, category: category);
            WriteToFile(LogLevel.Trace, exception, category: category);
        }

        /// <inheritdoc />
        public void CategorizedTrace(string category, Exception exception)
        {
            WriteToFile(LogLevel.Trace, exception, category: category);
        }

        /// <inheritdoc />
        public void CategorizedDebug(string category, string message)
        {
            WriteToFile(LogLevel.Debug, message, category: category);
        }

        /// <inheritdoc />
        public void CategorizedDebug(string category, string content, string identifier)
        {
            WriteToFile(LogLevel.Fatal, content, fileName: identifier, category: category);
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
            if (customFormat == SerializerFormat.None)
            {
                WriteToFile(
                    LogLevel.Debug,
                    (string)content.GetSerializer(),
                    fileName: identifier,
                    category: category
                );

                return;
            }

            WriteToFile(
                LogLevel.Debug,
                (string)content.GetCustomSerializer(customFormat),
                fileName: identifier,
                category: category
            );
        }

        /// <inheritdoc />
        public void Dispose()
        {
            currentFileStream.Close();
            currentFileStream.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Writes a log message to a specified file, including various metadata about the application and the log entry.
        /// </summary>
        /// <param name="level">The log level indicating the severity of the log message.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="category">Optional. The category of the log message. If not provided, a default category will be used.</param>
        /// <remarks>
        /// This method constructs a log message containing various details such as the application name, category, timestamp, hostname,
        /// unique identifier, IP addresses, log level, and the actual message content. It checks if the specified log level is enabled
        /// before proceeding to write the log entry. The method ensures that the log file does not exceed a certain size by updating
        /// the file stream as necessary. Depending on the specified file type, it formats the log message either as JSON or plain text
        /// before writing it to the current file stream. The method is thread-safe and uses a lock to prevent concurrent write operations.
        /// </remarks>
        private void WriteToFile(LogLevel level, Exception exception, string category = default)
        {
            var exMessage = new StringBuilder();

            exMessage.AppendFormat(
                CultureInfo.InvariantCulture,
                $"[MainException][{exception.GetType()}]: {exception.Message}{Environment.NewLine}"
            );
            exception = exception.InnerException;

            while (exception != null)
            {
                exMessage.AppendFormat(
                    CultureInfo.InvariantCulture,
                    $"[InnerException][{exception.GetType()}]: {exception.Message}{Environment.NewLine}"
                );
                exception = exception.InnerException;
            }

            exMessage.Length -= Environment.NewLine.Length;

            WriteToFile(level, exMessage.ToString(), category);
        }

        private void WriteToFile(
            LogLevel level,
            string content,
            string fileName = default,
            string category = default
        )
        {
            if (!_level.HasFlag(level))
            {
                return;
            }

            var message = new LogMessage()
            {
                Application = EnvironmentHelper.ApplicationName,
                Category = category == default ? _defaultCategory : category,
                Date = DateTime.Now,
                Hostname = EnvironmentHelper.Host,
                Id = Guid.NewGuid().ToString(),
                IpAddress = EnvironmentHelper.IpAddress,
                IpAddressRemote = EnvironmentHelper.IpAddressExternal,
                Level = level.GetHumanReadableValue(),
                Message = content,
                MessageIdentifier = fileName == default ? currentFileName : fileName,
                Operation = EnvironmentHelper.Operation,
                ProcessId = EnvironmentHelper.ProcessId,
                UserAgent = EnvironmentHelper.UserAgent,
                ThreadId = Environment.CurrentManagedThreadId,
                ThreadName = Thread.CurrentThread.Name,
            };

            lock (_syncRoot)
            {
                while (!IsValidSize(currentFileStream))
                {
                    UpdateFileStream();
                }

                byte[] messageBytes;

                switch (_fileType)
                {
                    case LogFileType.JSON:
                        messageBytes = WriteToJson(message);
                        break;

                    default:
                        messageBytes = WriteToTxt(message);
                        break;
                }

                currentFileStream.Write(messageBytes, 0, messageBytes.Length);
                currentFileStream.Flush();
                fileMessageCount++;
            }
        }

        private byte[] WriteToJson(LogMessage message)
        {
            var messageStr = (string)message.GetSerializer();
            byte[] messageBytes;

            if (fileMessageCount > 0)
            {
                currentFileStream.SetLength(
                    currentFileStream.Length - 1 - Environment.NewLine.Length
                );
                messageBytes = Encoding.UTF8.GetBytes(
                    $",{Environment.NewLine}{messageStr}{Environment.NewLine}]"
                );
            }
            else
            {
                messageBytes = Encoding.UTF8.GetBytes(
                    $"[{Environment.NewLine}{messageStr}{Environment.NewLine}]"
                );
            }

            return messageBytes;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Performance",
            "CA1822:Mark members as static",
            Justification = "Private method."
        )]
        private byte[] WriteToTxt(LogMessage message)
        {
            var props = typeof(LogMessage).GetProperties();
            var messageStr = new StringBuilder();
            string val;

            messageStr.AppendFormat(
                CultureInfo.InvariantCulture,
                $"{Environment.NewLine}----------{Environment.NewLine}"
            );

            foreach (var prop in props)
            {
                val =
                    prop.GetValue(message) == null
                    || prop.GetValue(message).ToString() == string.Empty
                        ? "N.A."
                        : prop.GetValue(message).ToString();

                messageStr.AppendFormat(
                    CultureInfo.InvariantCulture,
                    $"{prop.Name}: {val}{Environment.NewLine}"
                );
            }

            return Encoding.UTF8.GetBytes(messageStr.ToString());
        }

        private void UpdateFileStream()
        {
            currentFileStream.Close();
            currentFileStream.Dispose();

            CreateNewFileName();

            currentFileStream = GetFile();
        }

        private void CreateNewFileName()
        {
            fileMessageCount = 0;
            currentFileName =
                $"LogFile-{_fileNameSeed}-[{fileNumber}].{_fileType.GetInternalValue()}";

            while (File.Exists(Path.Combine(_folderPath, currentFileName)))
            {
                fileNumber++;
                currentFileName =
                    $"LogFile-{_fileNameSeed}-[{fileNumber}].{_fileType.GetInternalValue()}";
            }
        }

        private bool IsValidSize(FileStream stream)
        {
            stream.Flush();

            return (fileMessageCount < _maxMessageCount) && (stream.Length < _maxFileSize);
        }

        private FileStream GetFile()
        {
            return File.Create(Path.Combine(_folderPath, currentFileName));
        }
    }

    /// <summary>
    /// Unit enum for file size. Contains sizes for byte, kilobyte, megabyte and gigabyte.
    /// </summary>
    public enum Unit
    {
        /// <summary>
        /// Represents the unit byte.
        /// </summary>
        Byte = 1,

        /// <summary>
        /// Represents the unit kilo byte.
        /// </summary>
        KByte = 1024,

        /// <summary>
        /// Represents the unit mega byte.
        /// </summary>
        MByte = 1024 * 1024,

        /// <summary>
        /// Represents the unit giga byte.
        /// </summary>
        GByte = 1024 * 1024 * 1024,
    }

    /// <summary>
    /// Enum for the type of log file to be generated.
    /// </summary>
    public enum LogFileType
    {
        /// <summary>
        /// The .txt file type.
        /// </summary>
        [InternalValue("txt")]
        Text = 0,

        /// <summary>
        /// The .json file type.
        /// </summary>
        [InternalValue("json")]
        JSON,
    }
}

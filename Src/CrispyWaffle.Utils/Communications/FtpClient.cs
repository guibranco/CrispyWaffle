// ***********************************************************************
// Assembly         : CrispyWaffle.Utils
// Author           : Guilherme Branco Stracini
// Created          : 23/12/2022
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 22/03/2023
// ***********************************************************************
// <copyright file="FtpClient.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Diagnostics.CodeAnalysis;

namespace CrispyWaffle.Utils.Communications
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using Configuration;
    using CrispyWaffle.Extensions;
    using Log;
    using GoodPractices;

    /// <summary>
    /// Class FtpClient.
    /// </summary>
    public class FtpClient
    {
        #region Private fields

        /// <summary>
        /// The synchronize root.
        /// </summary>
        private readonly object _syncRoot = new();

        /// <summary>
        /// The host
        /// </summary>
        private readonly string _host;

        /// <summary>
        /// The port
        /// </summary>
        private readonly int _port;

        /// <summary>
        /// The user name
        /// </summary>
        private readonly string _userName;

        /// <summary>
        /// The password
        /// </summary>
        private readonly string _password;

        /// <summary>
        /// The sub directory
        /// </summary>
        private readonly string _remoteDirectory;

        /// <summary>
        /// The files.
        /// </summary>
        private readonly Queue<string> _files = new();

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClient" /> class.
        /// </summary>
        /// <param name="ftp">The FtpClient.</param>
        /// <param name="remoteDirectory">The remote directory.</param>
        public FtpClient(IConnection ftp, string remoteDirectory)
            : this(
                ftp?.Host,
                ftp?.Port ?? 0,
                ftp?.Credentials.UserName,
                ftp?.Credentials.Password,
                remoteDirectory
            )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClient" /> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="remoteDirectory">The sub directory.</param>
        /// <exception cref="System.ArgumentNullException">remoteDirectory</exception>
        [SuppressMessage("ReSharper", "TooManyDependencies")]
        public FtpClient(
            string host,
            int port,
            string userName,
            string password,
            string remoteDirectory
        )
        {
            if (string.IsNullOrWhiteSpace(remoteDirectory))
            {
                throw new ArgumentNullException(nameof(remoteDirectory));
            }

            _host = host;
            _port = port;
            _userName = userName;
            _password = password;
            _remoteDirectory = remoteDirectory;
            if (_remoteDirectory.EndsWith(@"/", StringComparison.InvariantCultureIgnoreCase))
            {
                _remoteDirectory = _remoteDirectory.Substring(0, _remoteDirectory.Length - 1);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Check if the path/file exists in the FtpClient host.
        /// </summary>
        /// <param name="path">The path String.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        /// <exception cref="System.InvalidOperationException">Response stream is null</exception>
        private bool ExistsInternal(string path)
        {
            var result = false;
            Stream responseStream = null;

            try
            {
                LogConsumer.Info(
                    "Checking in FtpClient the path/file: {0}",
                    path.GetPathOrFileName()
                );
                var uri = new Uri(path);
                var request = (FtpWebRequest)WebRequest.Create(uri);
                request.Credentials = new NetworkCredential(_userName, _password);
                request.Method = !string.IsNullOrWhiteSpace(uri.GetFileExtension())
                    ? WebRequestMethods.Ftp.GetFileSize
                    : WebRequestMethods.Ftp.ListDirectory;
                request.Timeout = 30000;
                request.ReadWriteTimeout = 90000;
                request.UsePassive = true;
                var response = (FtpWebResponse)request.GetResponse();
                var status = response.StatusCode;
                responseStream = response.GetResponseStream();
                if (responseStream == null)
                {
                    throw new InvalidOperationException("Response stream is null");
                }

                using var reader = new StreamReader(responseStream);
                responseStream = null;
                while (!reader.EndOfStream)
                {
                    _files.Enqueue(reader.ReadLine());
                }

                if (
                    !string.IsNullOrWhiteSpace(uri.GetFileExtension())
                        && status == FtpStatusCode.FileStatus
                    || status == FtpStatusCode.OpeningData
                )
                {
                    result = true;
                }
            }
            catch (WebException)
            {
                result = false;
            }
            finally
            {
                responseStream?.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Creates the file in the FtpClient host
        /// </summary>
        /// <param name="path">The path String.</param>
        /// <param name="bytes">The bytes.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        /// <exception cref="CrispyWaffle.Utils.GoodPractices.FtpClientException">create</exception>
        private bool CreateInternal(string path, byte[] bytes)
        {
            var result = false;
            try
            {
                LogConsumer.Info(
                    "Uploading to FtpClient the file: {0}",
                    path.GetPathOrFileName()
                );
                var uri = new Uri(path);
                var request = (FtpWebRequest)WebRequest.Create(uri);
                request.Credentials = new NetworkCredential(_userName, _password);
                request.UsePassive = true;
                if (!string.IsNullOrWhiteSpace(uri.GetFileExtension()))
                {
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.ContentLength = bytes.Length;
                    var stream = request.GetRequestStream();
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Close();
                }
                else
                {
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                }

                var response = (FtpWebResponse)request.GetResponse();
                if (
                    !string.IsNullOrWhiteSpace(uri.GetFileExtension())
                        && response.StatusCode == FtpStatusCode.ClosingData
                    || response.StatusCode == FtpStatusCode.PathnameCreated
                )
                {
                    result = true;
                }

                response.Close();
            }
            catch (WebException e)
            {
                throw new FtpClientException(path.GetPathOrFileName(), "create", e);
            }

            return result;
        }

        /// <summary>
        /// Removes the path/file in the FtpClient host.
        /// </summary>
        /// <param name="path">The path String.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        /// <exception cref="CrispyWaffle.Utils.GoodPractices.FtpClientException">remove</exception>
        private void RemoveInternal(string path)
        {
            try
            {
                LogConsumer.Info("Uploading to FtpClient the file: {0}", path.GetPathOrFileName());
                var fullPath = new Uri(path);
                var request = (FtpWebRequest)WebRequest.Create(fullPath);
                request.Credentials = new NetworkCredential(_userName, _password);
                request.Method = !string.IsNullOrWhiteSpace(fullPath.GetFileExtension())
                    ? WebRequestMethods.Ftp.DeleteFile
                    : WebRequestMethods.Ftp.RemoveDirectory;
                request.UsePassive = true;
                var response = (FtpWebResponse)request.GetResponse();
                if (response.StatusCode != FtpStatusCode.FileActionOK)
                {
                    throw new FtpClientException(
                        path.GetPathOrFileName(),
                        "remove",
                        response.StatusCode
                    );
                }
            }
            catch (WebException e)
            {
                throw new FtpClientException(path.GetPathOrFileName(), "remove", e);
            }
        }

        /// <summary>
        /// Gets the FtpClient URL.
        /// </summary>
        /// <returns>StringBuilder.</returns>
        private StringBuilder GetFtpUrl()
        {
            var str = new StringBuilder();
            return str.Append(@"ftp://")
                .Append(_host)
                .Append(@":")
                .Append(_port)
                .Append(@"/")
                .Append(_remoteDirectory)
                .Append(@"/");
        }

        /// <summary>
        /// Check if a file or directory exists in the FtpClient endpoint
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool Exists() => ExistsInternal(GetFtpUrl().ToString());

        /// <summary>
        /// Check if the path exists in the FtpClient endpoint
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool Exists(string path) => ExistsInternal(GetFtpUrl().Append(path).ToString());

        /// <summary>
        /// Removes this instance.
        /// </summary>
        private void Remove() => RemoveInternal(GetFtpUrl().ToString());

        /// <summary>
        /// Creates the directory.
        /// </summary>
        private void CreateDirectory() => CreateInternal(GetFtpUrl().ToString(), null);

        #endregion

        #region Public methods

        /// <summary>
        /// Removes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void Remove(string path)
        {
            lock (_syncRoot)
            {
                RemoveInternal(GetFtpUrl().Append(path).ToString());
            }
        }

        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CreateDirectory(string name)
        {
            lock (_syncRoot)
            {
                return CreateInternal(GetFtpUrl().Append(name).Append(@"/").ToString(), null);
            }
        }

        /// <summary>
        /// Uploads the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="bytes">The bytes.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">fileName</exception>
        /// <exception cref="System.ArgumentNullException">bytes</exception>
        public bool Upload(string fileName, byte[] bytes)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            lock (_syncRoot)
            {
                if (!Exists())
                {
                    CreateDirectory();
                }

                if (Exists(fileName))
                {
                    Remove(fileName);
                }

                return CreateInternal(GetFtpUrl().Append(fileName).ToString(), bytes);
            }
        }

        /// <summary>
        /// Clears the directory.
        /// </summary>
        public void ClearDirectory()
        {
            lock (_syncRoot)
            {
                _files.Clear();
                if (Exists())
                {
                    var counter = _files.Count;
                    for (var x = 0; x < counter; x++)
                    {
                        Remove(_files.Dequeue());
                    }

                    Remove();
                }

                CreateDirectory();
            }
        }

        #endregion
    }
}

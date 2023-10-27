// ***********************************************************************
// Assembly         : CrispyWaffle
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="EnvironmentHelper.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace CrispyWaffle.Infrastructure
{
    using Validations;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Class EnvironmentHelper.
    /// </summary>
    public static class EnvironmentHelper
    {
        #region ~Ctor

        /// <summary>
        /// Initializes static members of the <see cref="EnvironmentHelper"/> class.
        /// </summary>
        static EnvironmentHelper()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            ApplicationName = assembly.GetName().Name.Replace(@" ", @"-").ToLowerInvariant();
            Host = Environment.MachineName;
            UserName = Environment.UserName;
            IpAddress = GetIpAddressLocal();
            IpAddressExternal = GetIpAddressExternal();
            ExecutionPath = assembly.Location;
            Version = !string.IsNullOrWhiteSpace(ExecutionPath)
                ? FileVersionInfo.GetVersionInfo(ExecutionPath).ProductVersion
                : string.Empty;
            VersionDate = new FileInfo(ExecutionPath).LastWriteTime.ToString(
                @"dd/MM/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture
            );
            OperationalSystemVersion =
                $@"{Environment.OSVersion} - {(Environment.Is64BitOperatingSystem ? @"x64" : @"x86")}";
            ProcessId = Process.GetCurrentProcess().Id;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets the ip address local.
        /// </summary>
        /// <returns>System.String.</returns>
        private static string GetIpAddressLocal()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                foreach (var ip in ni.GetIPProperties().UnicastAddresses.Select(ip => ip.Address))
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the ip address external.
        /// </summary>
        /// <returns>System.String.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Style",
            "IDE0063:Use simple 'using' statement",
            Justification = "Not compatible with .Net Standard 2.0"
        )]
        private static string GetIpAddressExternal()
        {
            // ReSharper disable once ConvertToUsingDeclaration
            using (var wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                try
                {
                    var ip = wc.DownloadString("https://api.ipify.org");
                    return NetworkValidations.IpAddressPattern.IsMatch(ip)
                        ? NetworkValidations.IpAddressPattern.Match(ip).Value
                        : string.Empty;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Sets the name of the application.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        public static void SetApplicationName(string applicationName) =>
            ApplicationName = applicationName;

        /// <summary>
        /// Sets the display name of the application.
        /// </summary>
        /// <param name="displayApplicationName">Display name of the application.</param>
        public static void SetDisplayApplicationName(string displayApplicationName) =>
            DisplayApplicationName = displayApplicationName;

        /// <summary>
        /// Sets the operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        public static void SetOperation(string operation) => Operation = operation;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the host.
        /// </summary>
        /// <value>The host.</value>
        public static string Host { get; }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public static string UserName { get; }

        /// <summary>
        /// Gets the ip address.
        /// </summary>
        /// <value>The ip address.</value>
        public static string IpAddress { get; }

        /// <summary>
        /// Gets the ip address external.
        /// </summary>
        /// <value>The ip address external.</value>
        public static string IpAddressExternal { get; }

        /// <summary>
        /// Gets the execution path.
        /// </summary>
        /// <value>The execution path.</value>
        public static string ExecutionPath { get; }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <value>The name of the application.</value>
        public static string ApplicationName { get; private set; }

        /// <summary>
        /// Gets the display name of the application.
        /// </summary>
        /// <value>The display name of the application.</value>
        public static string DisplayApplicationName { get; private set; }

        /// <summary>
        /// Gets the operation.
        /// </summary>
        /// <value>The operation.</value>
        public static string Operation { get; private set; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public static string Version { get; }

        /// <summary>
        /// Gets the version date.
        /// </summary>
        /// <value>The version date.</value>
        public static string VersionDate { get; }

        /// <summary>
        /// Gets the operational system version.
        /// </summary>
        /// <value>The operational system version.</value>
        public static string OperationalSystemVersion { get; }

        /// <summary>
        /// Gets the process identifier.
        /// </summary>
        /// <value>The process identifier.</value>
        public static int ProcessId { get; }

        /// <summary>
        /// Gets the user agent.
        /// </summary>
        /// <value>The user agent.</value>
        [Localizable(false)]
        public static string UserAgent =>
            $"{ApplicationName}/{Version} (H:{Host}|P:{ProcessId}|T:{Environment.CurrentManagedThreadId})";

        #endregion
    }
}

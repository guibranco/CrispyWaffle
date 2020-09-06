// ***********************************************************************
// Assembly         : CrispyWaffle.Configuration
// Author           : Guilherme Branco Stracini
// Created          : 09-03-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-06-2020
// ***********************************************************************
// <copyright file="Credentials.cs" company="Guilherme Branco Stracini ME">
//     © 2020 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using CrispyWaffle.Composition;
using CrispyWaffle.Cryptography;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace CrispyWaffle.Configuration
{
    /// <summary>
    /// The credentials class.
    /// </summary>
    /// <seealso cref="IConnectionCredential" />
    /// <seealso cref="INotifyPropertyChanged" />
    public sealed class Credentials : IConnectionCredential, INotifyPropertyChanged
    {
        #region Implementation of INotifyPropertyChanged

        /// <summary>
        /// The property changed event handler
        /// </summary>
        /// <returns></returns>

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// OnPropertyChanged event invoker
        /// </summary>
        /// <param name="propertyName">The property name that was changed</param>

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// The user name
        /// </summary>
        private string _userName;

        /// <summary>
        /// The password
        /// </summary>
        private string _password;


        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        /// <value>The user name.</value>

        [Localizable(false)]
        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>

        [XmlIgnore]
        [JsonIgnore]
        [Localizable(false)]
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        /// <summary>
        /// Safe-to-store password, encrypted and hashed validated
        /// </summary>
        /// <value>The password internal.</value>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement("Password")]
        [JsonProperty("Password")]
        [Localizable(false)]
        public string PasswordInternal
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_password))
                {
                    return string.Empty;
                }

                var secureProvider = ServiceLocator.Resolve<ISecureCredentialProvider>();

                var encrypt = _password.Encrypt(secureProvider.PasswordHash, secureProvider.SaltKey, secureProvider.ViKey);
                return $"{encrypt}{Security.Hash(encrypt, HashAlgorithmType.MD5)}";
            }
            set
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return;
                    }

                    if (value.Length <= 32)
                    {
                        _password = value;
                    }

                    var password = value.Substring(0, value.Length - 32);

                    var md5 = value.Substring(value.Length - 32);

                    var check = Security.Hash(password, HashAlgorithmType.MD5);

                    var secureProvider = ServiceLocator.Resolve<ISecureCredentialProvider>();

                    _password = 0 == StringComparer.OrdinalIgnoreCase.Compare(md5, check) ? password.Decrypt(secureProvider.PasswordHash, secureProvider.SaltKey, secureProvider.ViKey) : value;
                }
                catch (Exception)
                {
                    _password = value;
                }
                finally
                {
                    OnPropertyChanged(nameof(PasswordInternal));
                }
            }
        }
    }
}

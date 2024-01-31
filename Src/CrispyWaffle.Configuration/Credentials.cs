using System;
using System.ComponentModel;
using System.Xml.Serialization;
using CrispyWaffle.Composition;
using CrispyWaffle.Cryptography;
using Newtonsoft.Json;

namespace CrispyWaffle.Configuration
{
    /// <summary>
    /// The credentials class.
    /// </summary>
    /// <seealso cref="IConnectionCredential"/>
    /// <seealso cref="INotifyPropertyChanged"/>
    public sealed class Credentials : IConnectionCredential, INotifyPropertyChanged
    {
        /// <summary>
        /// The property changed event handler
        /// </summary>
        /// <returns></returns>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// OnPropertyChanged event invoker
        /// </summary>
        /// <param name="propertyName">The property name that was changed</param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// The username.
        /// </summary>
        private string _username;

        /// <summary>
        /// The password.
        /// </summary>
        private string _password;

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>

        [Localizable(false)]
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>

        [XmlIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
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
        [System.Text.Json.Serialization.JsonPropertyName("Password")]
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

                var encrypt = _password.Encrypt(
                    secureProvider.PasswordHash,
                    secureProvider.SaltKey,
                    secureProvider.IVKey
                );
                return $"{encrypt}{Security.Hash(encrypt, HashAlgorithmType.Md5)}";
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

                    var check = Security.Hash(password, HashAlgorithmType.Md5);

                    var secureProvider = ServiceLocator.Resolve<ISecureCredentialProvider>();

                    _password =
                        StringComparer.OrdinalIgnoreCase.Compare(md5, check) == 0
                            ? password.Decrypt(
                                secureProvider.PasswordHash,
                                secureProvider.SaltKey,
                                secureProvider.IVKey
                            )
                            : value;
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

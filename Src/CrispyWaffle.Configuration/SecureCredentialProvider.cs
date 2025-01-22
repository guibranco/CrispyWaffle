namespace CrispyWaffle.Configuration;

/// <summary>
/// Class SecureCredentialProvider.
/// </summary>
public class SecureCredentialProvider : ISecureCredentialProvider
{
    /// <summary>
    /// Gets or sets the password hash.
    /// </summary>
    /// <value>The password hash.</value>
    public string PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets the salt key.
    /// </summary>
    /// <value>The salt key.</value>
    public string SaltKey { get; set; }

    /// <summary>
    /// Gets or sets the initialization vector key.
    /// </summary>
    /// <value>The initialization vector key.</value>
    public string IVKey { get; set; }
}

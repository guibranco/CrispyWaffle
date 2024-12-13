namespace CrispyWaffle.Configuration;

/// <summary>
/// Interface ISecureCredentialProvider.
/// </summary>
public interface ISecureCredentialProvider
{
    /// <summary>
    /// Gets or sets the password hash.
    /// </summary>
    /// <value>The password hash.</value>
    string PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets the salt key.
    /// </summary>
    /// <value>The salt key.</value>
    string SaltKey { get; set; }

    /// <summary>
    /// Gets or sets the initialization vector key.
    /// </summary>
    /// <value>The initialization vector key.</value>
    string IVKey { get; set; }
}

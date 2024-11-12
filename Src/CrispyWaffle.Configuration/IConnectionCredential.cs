namespace CrispyWaffle.Configuration;

/// <summary>
/// Connection credential interface.
/// </summary>
public interface IConnectionCredential
{
    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    /// <value>The password.</value>
    string Password { get; set; }

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    /// <value>The username.</value>
    string Username { get; set; }
}

namespace CrispyWaffle.Utils.Communications
{
    /// <summary>
    /// Class SmtpMailerOptions.
    /// </summary>
    public class SmtpMailerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is sandbox.
        /// </summary>
        /// <value><c>true</c> if this instance is sandbox; otherwise, <c>false</c>.</value>
        public bool IsSandbox { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether [enable debug].
        /// </summary>
        /// <value><c>true</c> if [enable debug]; otherwise, <c>false</c>.</value>
        public bool EnableDebug { get; set; } = false;

        /// <summary>
        /// Gets or sets from address.
        /// </summary>
        /// <value>From address.</value>
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets from name.
        /// </summary>
        /// <value>From name.</value>
        public string FromName { get; set; }
    }
}

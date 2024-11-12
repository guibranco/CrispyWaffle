namespace CrispyWaffle.Utilities
{
    /// <summary>
    /// Represents a phone number consisting of a country code, region code, and the phone number itself.
    /// </summary>
    public struct PhoneNumber
    {
        /// <summary>
        /// Gets the country code for the phone number.
        /// </summary>
        /// <value>
        /// The country code associated with the phone number.
        /// </value>
        public int CountryCode { get; private set; }

        /// <summary>
        /// Gets the region code for the phone number.
        /// </summary>
        /// <value>
        /// The region code associated with the phone number.
        /// </value>
        public int RegionCode { get; private set; }

        /// <summary>
        /// Gets the main phone number.
        /// </summary>
        /// <value>
        /// The main phone number without the country or region codes.
        /// </value>
        public long Number { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the phone number includes a ninth digit.
        /// </summary>
        /// <value>
        /// <c>true</c> if the phone number includes a ninth digit; otherwise, <c>false</c>.
        /// </value>
        public bool IsNinthDigit { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumber"/> struct with the specified country code, region code, and number.
        /// </summary>
        /// <param name="countryCode">The country code for the phone number (e.g., 1 for the United States, 55 for Brazil).</param>
        /// <param name="regionCode">The region code (e.g., area code or city code) for the phone number.</param>
        /// <param name="number">The actual phone number, excluding the country and region codes.</param>
        /// <remarks>
        /// This constructor also calculates whether the phone number has a ninth digit, which is specific to certain countries (e.g., Brazil).
        /// </remarks>
        public PhoneNumber(int countryCode, int regionCode, long number)
        {
            CountryCode = countryCode;
            RegionCode = regionCode;
            Number = number;
            IsNinthDigit = countryCode == 55 && number > 99999999; // Specific condition for Brazil (country code 55)
        }

        /// <summary>
        /// Returns a string representation of the phone number in the format of country code, region code, and number concatenated.
        /// </summary>
        /// <returns>A string representing the full phone number, including country code, region code, and the number.</returns>
        public override string ToString() => $@"{CountryCode}{RegionCode}{Number}";
    }
}

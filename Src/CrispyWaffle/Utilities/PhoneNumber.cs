namespace CrispyWaffle.Utilities
{
    /// <summary>
    /// Struct PhoneNumber
    /// </summary>
    public struct PhoneNumber
    {
        /// <summary>
        /// The country code
        /// </summary>
        public int CountryCode { get; private set; }

        /// <summary>
        /// The region code
        /// </summary>
        public int RegionCode { get; private set; }

        /// <summary>
        /// The number
        /// </summary>
        public long Number { get; private set; }

        /// <summary>
        /// The is ninth digit
        /// </summary>
        public bool IsNinthDigit { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumber"/> struct.
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <param name="regionCode">The region code.</param>
        /// <param name="number">The number.</param>
        public PhoneNumber(int countryCode, int regionCode, long number)
        {
            CountryCode = countryCode;
            RegionCode = regionCode;
            Number = number;
            IsNinthDigit = countryCode == 55 && number > 99999999;
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString()
        {
            return $@"{CountryCode}{RegionCode}{Number}";
        }
    }
}

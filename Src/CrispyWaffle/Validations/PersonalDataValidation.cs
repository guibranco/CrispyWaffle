namespace CrispyWaffle.Validations
{
    using System;
    using System.Globalization;
    using Extensions;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Class PersonalDataValidation.
    /// </summary>
    public static class PersonalDataValidation
    {
        /// <summary>
        /// The same number document pattern
        /// </summary>
        public static readonly Regex SameNumberDocumentPattern = new Regex(
            @"(\d)\1{10}",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(10)
        );

        /// <summary>
        /// Validates the email address.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns>String.</returns>
        /// <exception cref="InvalidEmailAddressException"></exception>
        /// <exception cref="InvalidEmailAddressException"></exception>
        public static string ValidateEmailAddress(this string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                throw new InvalidEmailAddressException(string.Empty);
            }

            var isValid = NetworkValidations.EmailAddressPattern.IsMatch(emailAddress);
            if (isValid && !emailAddress.EndsWith(@"combr"))
            {
                return emailAddress;
            }

            if (emailAddress.EndsWith(@"combr") || emailAddress.Contains(@","))
            {
                return emailAddress.Replace(@"combr", @"com.br").Replace(@",", @".");
            }

            throw new InvalidEmailAddressException(emailAddress);
        }

        /// <summary>
        /// Determines whether [is valid brazilian person document].
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        ///   <c>true</c> if [is valid brazilian person document] [the specified document]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidBrazilianPersonDocument(this string document)
        {
            var digits = document.CalculateBrazilianPersonDocumentDigits();
            return document.EndsWith(digits);
        }

        /// <summary>
        /// Determines whether [is valid brazilian corporate document].
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        ///   <c>true</c> if [is valid brazilian corporate document] [the specified document]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidBrazilianCorporateDocument(this string document)
        {
            var digits = document.CalculateBrazilianCorporateDocument();
            return document.EndsWith(digits);
        }

        /// <summary>
        /// Calculates the brazilian person document digits.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDocumentException">
        /// </exception>
        public static string CalculateBrazilianPersonDocumentDigits(this string document)
        {
            int[] multiplierFirstDigit = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplierSecondDigit = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            return CalculateDocumentDigits(
                document,
                Constants.BrazilianPersonDocumentName,
                Constants.BrazilianPersonDocumentFullLength,
                multiplierFirstDigit,
                multiplierSecondDigit
            );
        }

        /// <summary>
        /// Calculates the brazilian corporate document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDocumentException"></exception>
        public static string CalculateBrazilianCorporateDocument(this string document)
        {
            int[] multiplierFirstDigit = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplierSecondDigit = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            return CalculateDocumentDigits(
                document,
                Constants.BrazilianCorporateDocumentName,
                Constants.BrazilianCorporateDocumentFullLength,
                multiplierFirstDigit,
                multiplierSecondDigit
            );
        }

        /// <summary>
        /// Calculates the document digits.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="fullLength">The full length.</param>
        /// <param name="multiplierFirstDigit">The multiplier a.</param>
        /// <param name="multiplierSecondDigit">The multiplier b.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="CrispyWaffle.Validations.InvalidDocumentException"></exception>
        private static string CalculateDocumentDigits(
            string document,
            string documentType,
            int fullLength,
            int[] multiplierFirstDigit,
            int[] multiplierSecondDigit
        )
        {
            document = document.RemoveNonNumeric();

            if (document.Length != fullLength)
            {
                throw new InvalidDocumentException(documentType, document);
            }

            if (SameNumberDocumentPattern.IsMatch(document))
            {
                throw new InvalidDocumentException(documentType, document);
            }

            var subset = document.Substring(0, fullLength - 2);
            var rest = CalculateModule11(subset, multiplierFirstDigit);
            var digit = rest.ToString(CultureInfo.CurrentCulture);

            subset = string.Concat(subset, digit);

            rest = CalculateModule11(subset, multiplierSecondDigit);

            digit = string.Concat(digit, rest);

            return digit;
        }

        /// <summary>
        /// Calculates the module11.
        /// </summary>
        /// <param name="working">The working.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns>System.Int32.</returns>
        private static int CalculateModule11(string working, int[] multiplier)
        {
            var sum = 0;

            for (var i = 0; i < multiplier.Length; i++)
            {
                sum += working[i].ToString(CultureInfo.CurrentCulture).ToInt32() * multiplier[i];
            }

            var rest = sum % 11;

            rest = rest < 2 ? 0 : 11 - rest;

            return rest;
        }
    }
}

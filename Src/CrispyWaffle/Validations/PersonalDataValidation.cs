namespace CrispyWaffle.Validations
{
    using Extensions;
    using System.Text.RegularExpressions;

    public static class PersonalDataValidation
    {
        /// <summary>
        /// The same number document pattern
        /// </summary>
        public static readonly Regex SameNumberDocumentPattern = new Regex(@"(\d)\1{10}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

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
                throw new InvalidEmailAddressException(string.Empty);
            var isValid = NetworkValidations.EmailAddressPattern.IsMatch(emailAddress);
            if (isValid && !emailAddress.EndsWith(@"combr"))
                return emailAddress;
            if (emailAddress.EndsWith(@"combr") || emailAddress.Contains(@","))
                return emailAddress.Replace(@"combr", @"com.br").Replace(@",", @".");
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
            document = document.RemoveNonNumeric();
            var digits = document.CalculateBrazilianPersonDocumentDigits();
            return digits.Length == 2 && document.EndsWith(digits);
        }


        /// <summary>
        /// Determines whether [is valid brazilian corporation document].
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        ///   <c>true</c> if [is valid brazilian corporation document] [the specified document]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidBrazilianCorporationDocument(this string document)
        {
            document = document.RemoveNonNumeric();
            var digits = document.CalculateBrazilianCorporationDocument();
            return digits.Length == 2 && document.EndsWith(digits);
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
            int[] multiplierA = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplierB = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            document = document.RemoveNonNumeric();
            if (document.Length != 11)
                throw new InvalidDocumentException("CPF", document);
            if (SameNumberDocumentPattern.IsMatch(document))
                throw new InvalidDocumentException("CPF", document);
            var working = document.Substring(0, 9);
            var sum = 0;
            for (var i = 0; i < 9; i++)
                sum += working[i].ToString(StringExtensions.Culture).ToInt32() * multiplierA[i];
            var rest = sum % 11;
            rest = rest < 2 ? 0 : 11 - rest;
            var digit = rest.ToString(StringExtensions.Culture);
            working = string.Concat(working, digit);
            sum = 0;
            for (var i = 0; i < 10; i++)
                sum += working[i].ToString(StringExtensions.Culture).ToInt32() * multiplierB[i];
            rest = sum % 11;
            rest = rest < 2 ? 0 : 11 - rest;
            digit = string.Concat(digit, rest);
            return digit;
        }

        /// <summary>
        /// Calculates the brazilian corporation document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDocumentException"></exception>
        public static string CalculateBrazilianCorporationDocument(this string document)
        {
            int[] multiplierA = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplierB = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            document = document.RemoveNonNumeric();
            if (document.Length != 14)
                throw new InvalidDocumentException("CNPJ", document);
            var working = document.Substring(0, 12);
            var sum = 0;
            for (var i = 0; i < 12; i++)
                sum += int.Parse(working[i].ToString(StringExtensions.Culture)) * multiplierA[i];
            var rest = sum % 11;
            rest = rest < 2 ? 0 : 11 - rest;
            var digit = rest.ToString(StringExtensions.Culture);
            working = working + digit;
            sum = 0;
            for (var i = 0; i < 13; i++)
                sum += int.Parse(working[i].ToString(StringExtensions.Culture)) * multiplierB[i];
            rest = sum % 11;
            rest = rest < 2 ? 0 : 11 - rest;
            digit = string.Concat(digit, rest);
            return digit;
        }
    }
}

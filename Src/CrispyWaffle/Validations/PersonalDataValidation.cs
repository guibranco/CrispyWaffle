using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CrispyWaffle.Extensions;

namespace CrispyWaffle.Validations;

/// <summary>
/// Provides static methods for validating and calculating Brazilian personal and corporate documents,
/// as well as validating email addresses.
/// </summary>
public static class PersonalDataValidation
{
    /// <summary>
    /// A regular expression pattern that matches documents with the same repeated number (e.g., "11111111111").
    /// </summary>
    public static readonly Regex SameNumberDocumentPattern = new Regex(
        @"(\d)\1{10}",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(10)
    );

    /// <summary>
    /// Validates an email address by ensuring it follows a correct pattern and is properly formatted.
    /// If the email address ends with "combr" or contains a comma, it is adjusted accordingly.
    /// </summary>
    /// <param name="emailAddress">The email address to validate.</param>
    /// <returns>
    /// A valid email address string if the input is correctly formatted.
    /// </returns>
    /// <exception cref="InvalidEmailAddressException">
    /// Thrown when the provided email address is not valid.
    /// </exception>
    public static string ValidateEmailAddress(this string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            throw new InvalidEmailAddressException(string.Empty);
        }

        var isValid = NetworkValidations.EmailAddressPattern.IsMatch(emailAddress);
        if (isValid && !emailAddress.EndsWith("combr", StringComparison.OrdinalIgnoreCase))
        {
            return emailAddress;
        }

        if (
            emailAddress.EndsWith("combr", StringComparison.OrdinalIgnoreCase)
            || emailAddress.Contains(",")
        )
        {
            return emailAddress.Replace("combr", "com.br").Replace(",", ".");
        }

        throw new InvalidEmailAddressException(emailAddress);
    }

    /// <summary>
    /// Validates whether a given document is a valid Brazilian person document (e.g., CPF).
    /// </summary>
    /// <param name="document">The document number (CPF) to validate.</param>
    /// <returns>
    /// <c>true</c> if the document is valid; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValidBrazilianPersonDocument(this string document)
    {
        var digits = document.CalculateBrazilianPersonDocumentDigits();
        return document.EndsWith(digits, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates whether a given document is a valid Brazilian corporate document (e.g., CNPJ).
    /// </summary>
    /// <param name="document">The document number (CNPJ) to validate.</param>
    /// <returns>
    /// <c>true</c> if the document is valid; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValidBrazilianCorporateDocument(this string document)
    {
        var digits = document.CalculateBrazilianCorporateDocument();
        return document.EndsWith(digits, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Calculates the valid digits for a Brazilian person document (e.g., CPF).
    /// </summary>
    /// <param name="document">The document number (CPF) to calculate the digits for.</param>
    /// <returns>
    /// The calculated digits for the Brazilian person document (CPF).
    /// </returns>
    /// <exception cref="InvalidDocumentException">
    /// Thrown when the document is invalid or has an incorrect length.
    /// </exception>
    public static string CalculateBrazilianPersonDocumentDigits(this string document)
    {
        int[] multiplierFirstDigit = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplierSecondDigit = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        return CalculateDocumentDigits(
            document,
            Constants._BrazilianPersonDocumentName,
            Constants._BrazilianPersonDocumentFullLength,
            multiplierFirstDigit,
            multiplierSecondDigit
        );
    }

    /// <summary>
    /// Calculates the valid digits for a Brazilian corporate document (e.g., CNPJ).
    /// </summary>
    /// <param name="document">The document number (CNPJ) to calculate the digits for.</param>
    /// <returns>
    /// The calculated digits for the Brazilian corporate document (CNPJ).
    /// </returns>
    /// <exception cref="InvalidDocumentException">
    /// Thrown when the document is invalid or has an incorrect length.
    /// </exception>
    public static string CalculateBrazilianCorporateDocument(this string document)
    {
        int[] multiplierFirstDigit = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplierSecondDigit = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        return CalculateDocumentDigits(
            document,
            Constants._BrazilianCorporateDocumentName,
            Constants._BrazilianCorporateDocumentFullLength,
            multiplierFirstDigit,
            multiplierSecondDigit
        );
    }

    /// <summary>
    /// Calculates the verification digits for a Brazilian document (CPF or CNPJ) based on the provided multipliers.
    /// </summary>
    /// <param name="document">The document number (CPF or CNPJ) to calculate the digits for.</param>
    /// <param name="documentType">The type of document (e.g., "CPF" or "CNPJ").</param>
    /// <param name="fullLength">The expected full length of the document.</param>
    /// <param name="multiplierFirstDigit">The multipliers for the first digit calculation.</param>
    /// <param name="multiplierSecondDigit">The multipliers for the second digit calculation.</param>
    /// <returns>
    /// The calculated digits for the Brazilian document.
    /// </returns>
    /// <exception cref="InvalidDocumentException">
    /// Thrown when the document has an incorrect length or contains invalid data.
    /// </exception>
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
    /// Calculates the modulus 11 of a string of digits using a given multiplier array.
    /// </summary>
    /// <param name="working">The string of digits to process.</param>
    /// <param name="multiplier">The multiplier array used for the calculation.</param>
    /// <returns>
    /// The result of the modulus 11 calculation.
    /// </returns>
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

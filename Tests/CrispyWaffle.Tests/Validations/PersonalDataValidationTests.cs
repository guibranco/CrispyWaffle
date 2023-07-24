// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 21/03/2023
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 21/03/2023
// ***********************************************************************
// <copyright file="PersonalDataValidationTests.cs" company="CrispyWaffle.Tests">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.Tests.Validations
{
    using CrispyWaffle.Validations;
    using Xunit;

    /// <summary>
    /// Class PersonalDataValidationTests.
    /// </summary>
    public class PersonalDataValidationTests
    {
        /// <summary>
        /// Defines the test method ValidateEmailAddress_ValidEmailAddress_ReturnsValidEmailAddress.
        /// </summary>
        /// <param name="inputEmail">The input email.</param>
        /// <param name="expectedEmail">The expected email.</param>
        [Theory]
        [InlineData("john.doe@example.com", "john.doe@example.com")]
        [InlineData("nobody@example.combr", "nobody@example.com.br")]
        [InlineData("nobody@example,com", "nobody@example.com")]
        [InlineData("jane.doe.123456@example.com", "jane.doe.123456@example.com")]
        public void ValidateEmailAddress_ValidEmailAddress_ReturnsValidEmailAddress(
            string inputEmail,
            string expectedEmail
        )
        {
            var result = inputEmail.ValidateEmailAddress();
            Assert.Equal(expectedEmail, result);
        }

        /// <summary>
        /// Defines the test method ValidateEmailAddress_InvalidEmailAddress_ThrowsException.
        /// </summary>
        /// <param name="inputEmail">The input email.</param>
        [Theory]
        [InlineData("")]
        [InlineData("invalid.address")]
        [InlineData("test@@domain.com")]
        public void ValidateEmailAddress_InvalidEmailAddress_ThrowsException(string inputEmail)
        {
            var exception = Assert.Throws<InvalidEmailAddressException>(
                inputEmail.ValidateEmailAddress
            );
            Assert.Equal(
                $"The e-mail address '{inputEmail}' isn't in a valid e-mail address format",
                exception.Message
            );
        }

        /// <summary>
        /// Defines the test method IsValidBrazilianPersonDocument_ValidInput_ReturnsTrue.
        /// </summary>
        /// <param name="inputDocument">The input document.</param>
        [Theory]
        [InlineData("22970619008")]
        [InlineData("13511749085")]
        [InlineData("47428440092")]
        [InlineData("43045197000")]
        [InlineData("91290461066")]
        public void IsValidBrazilianPersonDocument_ValidInput_ReturnsTrue(string inputDocument)
        {
            var result = inputDocument.IsValidBrazilianPersonDocument();
            Assert.True(result);
        }

        /// <summary>
        /// Defines the test method IsValidBrazilianPersonDocument_InvalidInput_ThrowsException.
        /// </summary>
        /// <param name="inputDocument">The input document.</param>
        [Theory]
        [InlineData("")]
        [InlineData("11111111111")]
        [InlineData("22222222222")]
        [InlineData("33333333333")]
        [InlineData("44444444444")]
        [InlineData("55555555555")]
        [InlineData("66666666666")]
        [InlineData("77777777777")]
        [InlineData("88888888888")]
        [InlineData("99999999999")]
        [InlineData("00000000000")]
        [InlineData("1")]
        public void IsValidBrazilianPersonDocument_InvalidInput_ThrowsException(
            string inputDocument
        )
        {
            var exception = Assert.Throws<InvalidDocumentException>(
                () => inputDocument.IsValidBrazilianPersonDocument()
            );
            Assert.Equal(
                $"The value '{inputDocument}' isn't a valid value for a document of type CPF",
                exception.Message
            );
        }

        /// <summary>
        /// Defines the test method IsValidBrazilianCorporateDocument_ValidInput_ReturnsTrue.
        /// </summary>
        /// <param name="inputDocument">The input document.</param>
        [Theory]
        [InlineData("19664626000120")]
        [InlineData("82403744000111")]
        [InlineData("06010416000177")]
        [InlineData("22295895000171")]
        [InlineData("48382658000131")]
        public void IsValidBrazilianCorporateDocument_ValidInput_ReturnsTrue(string inputDocument)
        {
            var result = inputDocument.IsValidBrazilianCorporateDocument();
            Assert.True(result);
        }

        /// <summary>
        /// Defines the test method IsValidBrazilianCorporateDocument_InvalidInput_ThrowsException.
        /// </summary>
        /// <param name="inputDocument">The input document.</param>
        [Theory]
        [InlineData("")]
        [InlineData("11111111111111")]
        [InlineData("22222222222222")]
        [InlineData("33333333333333")]
        [InlineData("44444444444444")]
        [InlineData("55555555555555")]
        [InlineData("66666666666666")]
        [InlineData("77777777777777")]
        [InlineData("88888888888888")]
        [InlineData("99999999999999")]
        [InlineData("00000000000000")]
        [InlineData("1")]
        public void IsValidBrazilianCorporateDocument_InvalidInput_ThrowsException(
            string inputDocument
        )
        {
            var exception = Assert.Throws<InvalidDocumentException>(
                () => inputDocument.IsValidBrazilianCorporateDocument()
            );
            Assert.Equal(
                $"The value '{inputDocument}' isn't a valid value for a document of type CNPJ",
                exception.Message
            );
        }
    }
}

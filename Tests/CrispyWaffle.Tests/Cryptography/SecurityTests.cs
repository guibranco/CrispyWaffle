namespace CrispyWaffle.Tests.Cryptography
{
    using System;
    using CrispyWaffle.Cryptography;
    using Xunit;

    [Collection("Cryptography collection")]
    public class SecurityTests
    {
        private const string _passwordHash = "123456";
        private const string _saltKey = "12345678";
        private const string _viKey = "HZEM7|Ne2YGS/F41";

        private const string _plainText = nameof(_plainText);
        private const string _encryptedText = "9WfrFwgvu4EBtS0ZnAtdxg==";

        [Fact]
        public void Encrypt_Success()
        {
            var result = Security.Encrypt(_plainText, _passwordHash, _saltKey, _viKey);
            Assert.Equal(_encryptedText, result);
        }

        [Fact]
        public void Decrypt_Success()
        {
            var result = Security.Decrypt(_encryptedText, _passwordHash, _saltKey, _viKey);
            Assert.Equal(_plainText, result);
        }

        [Theory]
        [InlineData(HashAlgorithmType.Md5, "602968daf9a7f6497c955344de16047d")]
        [InlineData(HashAlgorithmType.Sha1, "38681531422580109214230134523724815720823120684138176")]
        [InlineData(HashAlgorithmType.Sha256, "243242351754210187222161691478133129221821245594511281561221051933421820827218108")]
        [InlineData(HashAlgorithmType.Sha384, "18819110966203230156111301901121931672079821097524314857472551623171901902459320284234212024711253019014820160145161986531")]
        [InlineData(HashAlgorithmType.Sha512, "33011189245236235605414944391021929124697831103203240125401495516621616410116812412413602221664610420200233113187224130153246152108180216236250118250186791341485154204")]
        public void Hash_Success(HashAlgorithmType type, string expectedHash)
        {
            var result = Security.Hash(_plainText, type);
            Assert.Equal(expectedHash, result);
        }

        [Fact]
        public void Hash_InvalidType()
        {
            var result = Assert.Throws<ArgumentOutOfRangeException>(() => Security.Hash(_plainText, (HashAlgorithmType)10));

            Assert.NotNull(result);
            Assert.Equal("Invalid algorithm type (Parameter 'type')\r\nActual value was 10.", result.Message);
        }
    }
}

namespace CrispyWaffle.Tests.Cryptography
{
    using System;
    using CrispyWaffle.Cryptography;
    using Xunit;

    [Collection("Cryptography collection")]
    public class SecurityTests
    {
        private const string PasswordHash = "123456";
        private const string SaltKey = "12345678";
        private const string ViKey = "HZEM7|Ne2YGS/F41";

        private const string PlainText = nameof(PlainText);
        private const string EncryptedText = "vBWW26e4f9QMRPjz4pXoEQ==";

        private const string Md5Hash = "b7ebbf7f254ef646928dd58f62383a85";
        private const string Sha1Hash = "5022439192242211661341747135888248197192199115101";

        private const string Sha256Hash =
            "43167247186156224140100585014234839723255150225212687216120565623113068401650";

        private const string Sha384Hash =
            "1971058120111222536871982354785921432042623733177161211966251132776513520695114214013371701731755877276711471261717729";

        private const string Sha512Hash =
            "2304016719818519658114181159661868266138285522924115715132174881042521481379823013415016323524141612161012211128317913263910513223439177961751431295019699470229117194212";

        [Fact]
        public void Encrypt_Success()
        {
            var result = Security.Encrypt(PlainText, PasswordHash, SaltKey, ViKey);
            Assert.Equal(EncryptedText, result);
        }

        [Fact]
        public void Decrypt_Success()
        {
            var result = Security.Decrypt(EncryptedText, PasswordHash, SaltKey, ViKey);
            Assert.Equal(PlainText, result);
        }

        [Theory]
        [InlineData(HashAlgorithmType.Md5, Md5Hash)]
        [InlineData(HashAlgorithmType.Sha1, Sha1Hash)]
        [InlineData(HashAlgorithmType.Sha256, Sha256Hash)]
        [InlineData(HashAlgorithmType.Sha384, Sha384Hash)]
        [InlineData(HashAlgorithmType.Sha512, Sha512Hash)]
        public void Hash_Success(HashAlgorithmType type, string expectedHash)
        {
            var result = Security.Hash(PlainText, type);
            Assert.Equal(expectedHash, result);
        }

        [Fact]
        public void Hash_InvalidType()
        {
            var result = Assert.Throws<ArgumentOutOfRangeException>(
                () => Security.Hash(PlainText, (HashAlgorithmType)10)
            );

            Assert.NotNull(result);
            Assert.Equal(
                "Invalid algorithm type (Parameter 'type')\r\nActual value was 10.",
                result.Message
            );
        }
    }
}

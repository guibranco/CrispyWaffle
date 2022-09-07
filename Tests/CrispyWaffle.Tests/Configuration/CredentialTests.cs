namespace CrispyWaffle.Tests.Configuration
{
    using CrispyWaffle.Composition;
    using CrispyWaffle.Configuration;
    using CrispyWaffle.Cryptography;
    using CrispyWaffle.Infrastructure;
    using CrispyWaffle.Serialization;
    using CrispyWaffle.Utilities;
    using Xunit;

    [Collection("Configuration collection")]
    public class CredentialTests
    {
        [Fact]
        public void ValidateSecureCredentialProvider()
        {
            var credential = new Credentials
            {
                Password = "DeltaBravoZulu",
                UserName = EnvironmentHelper.UserName
            };

            var json = (string)credential.GetCustomSerializer(SerializerFormat.Json);

            dynamic deserialized = SerializerFactory.GetCustomSerializer<DynamicSerialization>(SerializerFormat.Json).Deserialize(json);

            var passwordEncrypted = (string)deserialized.Password;

            passwordEncrypted = passwordEncrypted.Substring(0, passwordEncrypted.Length - 32);

            var secureCredentialProvider = ServiceLocator.Resolve<ISecureCredentialProvider>();

            var passwordDecrypted = passwordEncrypted.Decrypt(secureCredentialProvider.PasswordHash, secureCredentialProvider.SaltKey, secureCredentialProvider.IVKey);

            Assert.Equal(credential.Password, passwordDecrypted);

            Assert.Equal(credential.UserName, (string)deserialized.UserName);
        }

        [Fact]
        public void ValidateSecureCredentialProviderSetter()
        {
            var credential = new Credentials
            {
                Password = "EchoFoxPapa"
            };

            var json = (string)credential.GetCustomSerializer(SerializerFormat.Json);

            dynamic deserialized = SerializerFactory.GetCustomSerializer<DynamicSerialization>(SerializerFormat.Json).Deserialize(json);

            var passwordEncrypted = (string)deserialized.Password;

            var credentialResult = new Credentials
            {
                PasswordInternal = passwordEncrypted
            };

            Assert.Equal(credential.Password, credentialResult.Password);
        }

    }
}

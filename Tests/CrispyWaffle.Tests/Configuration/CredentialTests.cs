using CrispyWaffle.Composition;
using CrispyWaffle.Configuration;
using CrispyWaffle.Cryptography;
using CrispyWaffle.Infrastructure;
using CrispyWaffle.Serialization;
using CrispyWaffle.Utilities;
using Xunit;

namespace CrispyWaffle.Tests.Configuration;

[Collection("Configuration collection")]
public class CredentialTests
{
    /// <summary>
    /// Validates the secure credential provider by checking if the decrypted password matches the original password
    /// and if the username is correctly deserialized.
    /// </summary>
    /// <remarks>
    /// This test method creates a set of credentials with a predefined password and the current username.
    /// It then serializes these credentials to JSON format and deserializes them back into a dynamic object.
    /// The method retrieves the encrypted password from the deserialized object, removes the last 32 characters,
    /// and decrypts it using a secure credential provider's hashing parameters.
    /// Finally, it asserts that the decrypted password matches the original password and that the username
    /// is equal to the deserialized username. This ensures that the serialization and decryption processes
    /// are functioning correctly and securely.
    /// </remarks>
    [Fact]
    public void ValidateSecureCredentialProvider()
    {
        var credential = new Credentials
        {
            Password = "DeltaBravoZulu",
            Username = EnvironmentHelper.UserName,
        };

        var json = (string)credential.GetCustomSerializer(SerializerFormat.Json);

        dynamic deserialized = SerializerFactory
            .GetCustomSerializer<DynamicSerialization>(SerializerFormat.Json)
            .Deserialize(json);

        var passwordEncrypted = (string)deserialized.Password;

        passwordEncrypted = passwordEncrypted.Substring(0, passwordEncrypted.Length - 32);

        var secureCredentialProvider = ServiceLocator.Resolve<ISecureCredentialProvider>();

        var passwordDecrypted = passwordEncrypted.Decrypt(
            secureCredentialProvider.PasswordHash,
            secureCredentialProvider.SaltKey,
            secureCredentialProvider.IVKey
        );

        Assert.Equal(credential.Password, passwordDecrypted);

        Assert.Equal(credential.Username, (string)deserialized.Username);
    }

    [Fact]
    public void ValidateSecureCredentialProviderSetter()
    {
        var credential = new Credentials { Password = "EchoFoxPapa" };

        var json = (string)credential.GetCustomSerializer(SerializerFormat.Json);

        dynamic deserialized = SerializerFactory
            .GetCustomSerializer<DynamicSerialization>(SerializerFormat.Json)
            .Deserialize(json);

        var passwordEncrypted = (string)deserialized.Password;

        var credentialResult = new Credentials { PasswordInternal = passwordEncrypted };

        Assert.Equal(credential.Password, credentialResult.Password);
    }
}

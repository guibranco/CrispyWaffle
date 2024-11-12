using System;
using System.Diagnostics.CodeAnalysis;
using CrispyWaffle.Composition;
using CrispyWaffle.Configuration;

namespace CrispyWaffle.Tests.Fixtures;

[ExcludeFromCodeCoverage]
public class ConfigurationFixture : IDisposable
{
    public ConfigurationFixture()
    {
        ServiceLocator.Register<ISecureCredentialProvider>(
            () =>
                new SecureCredentialProvider
                {
                    PasswordHash = "Cr1$PTVV@FE13",
                    SaltKey = "y48H85nH21",
                    IVKey = "HZEM7|Ne2YGS/F41",
                },
            Lifetime.Singleton
        );
    }

    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
        {
            return;
        }

        if (disposing) { }

        _disposedValue = true;
    }

    public void Dispose() => Dispose(true);
}

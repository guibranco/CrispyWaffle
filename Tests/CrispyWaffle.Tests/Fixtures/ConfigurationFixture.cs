// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 09-06-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-06-2020
// ***********************************************************************
// <copyright file="ConfigurationFixture.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Diagnostics.CodeAnalysis;
using CrispyWaffle.Composition;
using CrispyWaffle.Configuration;

namespace CrispyWaffle.Tests.Fixtures;

/// <summary>
/// Class ConfigurationFixture.
/// Implements the <see cref="System.IDisposable" />
/// </summary>
/// <seealso cref="System.IDisposable" />
[ExcludeFromCodeCoverage]
public class ConfigurationFixture : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationFixture" /> class.
    /// </summary>
    public ConfigurationFixture()
    {
        ServiceLocator.Register<ISecureCredentialProvider>(
            () =>
                new SecureCredentialProvider
                {
                    PasswordHash = "Cr1$PTVV@FE13",
                    SaltKey = "y48H85nH21",
                    IVKey = "HZEM7|Ne2YGS/F41"
                },
            LifeStyle.Singleton
        );
    }

    #region IDisposable Support

    /// <summary>
    /// The disposed value
    /// </summary>
    private bool _disposedValue; // To detect redundant calls

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
        {
            return;
        }

        if (disposing)
        {
            //ServiceLocator.DisposeAllRegistrations();
        }

        _disposedValue = true;
    }

    // This code added to correctly implement the disposable pattern.
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
    }

    #endregion
}

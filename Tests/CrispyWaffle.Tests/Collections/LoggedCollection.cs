using CrispyWaffle.Tests.Fixtures;
using Xunit;

namespace CrispyWaffle.Tests.Collections;

/// <summary>
/// Class ServiceLocatorCollection.
/// Implements the <see cref="Xunit.ICollectionFixture{BootstrapFixture}" />
/// </summary>
/// <seealso cref="Xunit.ICollectionFixture{BootstrapFixture}" />
[CollectionDefinition("Logged collection")]
public class LoggedCollection : ICollectionFixture<LoggingFixture>
{
}

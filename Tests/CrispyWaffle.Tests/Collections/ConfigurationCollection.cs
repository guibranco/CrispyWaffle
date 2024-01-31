using CrispyWaffle.Tests.Fixtures;
using Xunit;

namespace CrispyWaffle.Tests.Collections;

/// <summary>
/// Class ConfigurationCollection.
/// Implements the <see cref="Xunit.ICollectionFixture{TFixture}" />
/// </summary>
/// <seealso cref="Xunit.ICollectionFixture{TFixture}" />
[CollectionDefinition("Configuration collection")]
public class ConfigurationCollection : ICollectionFixture<ConfigurationFixture>
{
}

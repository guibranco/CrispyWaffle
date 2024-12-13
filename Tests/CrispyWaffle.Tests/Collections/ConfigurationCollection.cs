using CrispyWaffle.Tests.Fixtures;
using Xunit;

namespace CrispyWaffle.Tests.Collections;

[CollectionDefinition("Configuration collection")]
public class ConfigurationCollection : ICollectionFixture<ConfigurationFixture> { }

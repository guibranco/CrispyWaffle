using CrispyWaffle.Tests.Fixtures;
using Xunit;

namespace CrispyWaffle.Tests.Collections;

[CollectionDefinition("Logged collection")]
public class LoggedCollection : ICollectionFixture<LoggingFixture> { }

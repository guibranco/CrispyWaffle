﻿using System;
using CrispyWaffle.Tests.Fixtures;
using CrispyWaffle.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace CrispyWaffle.Tests.Utilities;

[Collection("Logged collection")]
public class LazyResettableTests
{
    public LazyResettableTests(ITestOutputHelper testOutputHelper, LoggingFixture fixture) =>
        fixture.SetLogProvider(testOutputHelper);

    [Fact]
    public void LazyResettableSuccess()
    {
        var guid = Guid.NewGuid();

        var test = new LazyResettable<TestClass>(() => new TestClass { Id = guid, Count = 10 });

        Assert.NotNull(test);

        Assert.False(test.IsValueCreated);

        Assert.Equal(guid, test.Value.Id);
        Assert.Equal(10, test.Value.Count);

        Assert.True(test.IsValueCreated);

        test.Value.Count = 200;

        Assert.Equal(200, test.Value.Count);

        test.Reset();

        Assert.Equal(10, test.Value.Count);

        var stats = test.Stats();

        Assert.Equal(1, stats.Resets);
        Assert.Equal(2, stats.Loads);
        Assert.Equal(3, stats.Hits);
    }
}

public class TestClass
{
    public Guid Id { get; set; }
    public int Count { get; set; }
}

using CrispyWaffle.Log;
using CrispyWaffle.Log.Adapters;
using CrispyWaffle.Log.Filters;
using CrispyWaffle.Log.Providers;
using NSubstitute;
using Xunit;

namespace CrispyWaffle.Tests.Log.Providers;

public sealed class ConsoleLogProviderAndCategoryLogFilterTests
{
    [Fact]
    public void ConsoleLogProviderInstancesUseTheirOwnAdapters()
    {
        var firstAdapter = Substitute.For<IConsoleLogAdapter>();
        var secondAdapter = Substitute.For<IConsoleLogAdapter>();

        var firstProvider = new ConsoleLogProvider(firstAdapter);
        var secondProvider = new ConsoleLogProvider(secondAdapter);

        secondProvider.Info("cat", "to second");
        firstProvider.Info("cat", "to first");

        firstAdapter.Received(1).Info("to first");
        firstAdapter.DidNotReceive().Info("to second");
        secondAdapter.Received(1).Info("to second");
        secondAdapter.DidNotReceive().Info("to first");
    }

    [Fact]
    public void CategoryLogFilterExcludesCategoriesWhenExclusiveIsTrue()
    {
        var filter = new CategoryLogFilter(null, isExclusive: true).AddCategory("A");

        Assert.False(filter.Filter("x", LogLevel.Info, "A", "m"));
        Assert.True(filter.Filter("x", LogLevel.Info, "B", "m"));
    }

    [Fact]
    public void CategoryLogFilterIncludesOnlyListedCategoriesWhenExclusiveIsFalse()
    {
        var filter = new CategoryLogFilter(typeof(ConsoleLogProvider), isExclusive: false).AddCategory("A");

        Assert.True(filter.Filter(typeof(ConsoleLogProvider).FullName, LogLevel.Info, "A", "m"));
        Assert.False(filter.Filter(typeof(ConsoleLogProvider).FullName, LogLevel.Info, "B", "m"));
        Assert.True(filter.Filter(typeof(TextFileLogProvider).FullName, LogLevel.Info, "B", "m"));
    }
}

using System;
using System.Threading.Tasks;
using CrispyWaffle.Commands;
using Xunit;
using static CrispyWaffle.Tests.Commands.TestObjects;

namespace CrispyWaffle.Tests.Commands;

/// <summary>
/// Class CommandsTests.
/// </summary>
public class CommandsTests
{
    /// <summary>
    /// Defines the test method ValidateRaiseCommand
    /// </summary>
    [Fact]
    public void ValidateRaiseCommand()
    {
        var command = new TestDoneCommand(Guid.NewGuid(), "Sample test");
        var textResult =
            $"Sample done action handled: {command.Identifier} - {command.Text} - {command.CreatedDateTime:dd/MM/yyyy HH:mm:ss}";
        var result = CommandsConsumer.Raise<TestDoneCommand, TestDoneResultCommand>(command);
        Assert.Equal(textResult, result.Text);
    }

    /// <summary>
    /// Defines the test method ValidateRaiseAsyncCommand
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ValidateRaiseCommandAsync()
    {
        var command = new TestDoneCommand(Guid.NewGuid(), "Sample test");
        var textResult =
            $"Sample done action handled: {command.Identifier} - {command.Text} - {command.CreatedDateTime:dd/MM/yyyy HH:mm:ss}";
        var result = await CommandsConsumer.RaiseAsync<TestDoneCommand, TestDoneResultCommand>(
            command
        );
        Assert.Equal(textResult, result.Text);
    }
}

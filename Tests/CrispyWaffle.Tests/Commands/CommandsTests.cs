using System;
using System.Threading.Tasks;
using CrispyWaffle.Commands;
using Xunit;
using static CrispyWaffle.Tests.Commands.TestObjects;

namespace CrispyWaffle.Tests.Commands;

public class CommandsTests
{
    [Fact]
    public void ValidateRaiseCommand()
    {
        var command = new TestDoneCommand(Guid.NewGuid(), "Sample test");
        var textResult =
            $"Sample done action handled: {command.Identifier} - {command.Text} - {command.CreatedDateTime:dd/MM/yyyy HH:mm:ss}";
        var result = CommandsConsumer.Raise<TestDoneCommand, TestDoneResultCommand>(command);
        Assert.Equal(textResult, result.Text);
    }

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

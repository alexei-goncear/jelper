using System;
using System.Collections.Generic;
using System.Linq;
using Jelper.Commands;

namespace Jelper.Infrastructure;

internal sealed class InteractiveShell
{
    private readonly InputReader _input;
    private readonly IReadOnlyList<IConsoleCommand> _commands;

    public InteractiveShell(InputReader input, IEnumerable<IConsoleCommand> commands)
    {
        _input = input;
        _commands = commands.ToList();
    }

    public void Run()
    {
        Console.WriteLine($"Jelper ready. Type '{_input.ExitKeyword}' anywhere to cancel.");

        while (true)
        {
            ShowLegend();

            try
            {
                var command = ReadCommandSelection();
                command.Describe();
                command.Execute();
                Console.WriteLine("Done. Back to the legend.");
            }
            catch (UserExitRequestedException)
            {
                Console.WriteLine("Canceled.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error: {ex.Message}");
                Console.Error.WriteLine("Try again when ready.");
            }
        }
    }

    private void ShowLegend()
    {
        Console.WriteLine();
        Console.WriteLine("============= COMMANDS =============");
        foreach (var command in _commands)
        {
            Console.WriteLine($"{command.Key} - {command.Title,-17}: {command.Description}");
        }

        Console.WriteLine($"Type a number or name. '{_input.ExitKeyword}' returns here.");
        Console.WriteLine("====================================");
    }

    private IConsoleCommand ReadCommandSelection()
    {
        while (true)
        {
            var input = _input.ReadRequiredInput("Choose a command");
            var command = _commands.FirstOrDefault(c => c.Matches(input));
            if (command is not null)
            {
                return command;
            }

            Console.WriteLine("Unknown command. Pick one from the list.");
        }
    }
}

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
        Console.WriteLine("Interactive file helper ready. Close the window manually when finished.");
        Console.WriteLine($"Type '{_input.ExitKeyword}' at any prompt to return to the command legend.");

        while (true)
        {
            ShowLegend();

            try
            {
                var command = ReadCommandSelection();
                command.Describe();
                command.Execute();
                Console.WriteLine("Action complete. Returning to the legend.");
            }
            catch (UserExitRequestedException)
            {
                Console.WriteLine("Exit requested. Returning to the legend without running the command.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error: {ex.Message}");
                Console.Error.WriteLine("The console stays open. Please try again.");
            }
        }
    }

    private void ShowLegend()
    {
        Console.WriteLine();
        Console.WriteLine("================ COMMAND LEGEND ================");
        foreach (var command in _commands)
        {
            Console.WriteLine($"{command.Key} - {command.Title,-17}: {command.Description}");
        }

        Console.WriteLine($"Type the number or command name. Type '{_input.ExitKeyword}' at any prompt to return here.");
        Console.WriteLine("================================================");
    }

    private IConsoleCommand ReadCommandSelection()
    {
        while (true)
        {
            var input = _input.ReadRequiredInput("Enter the command number or name from the legend:");
            var command = _commands.FirstOrDefault(c => c.Matches(input));
            if (command is not null)
            {
                return command;
            }

            Console.WriteLine("Unknown command. Please choose one of the listed options.");
        }
    }
}

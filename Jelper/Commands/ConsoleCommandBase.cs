using System;
using Jelper.Infrastructure;
using Jelper.Services;

namespace Jelper.Commands;

internal abstract class ConsoleCommandBase : IConsoleCommand
{
    private readonly string[] _aliases;

    protected ConsoleCommandBase(InputReader input, ImageOperations operations, params string[] aliases)
    {
        Input = input;
        Operations = operations;
        _aliases = aliases;
    }

    protected InputReader Input { get; }
    protected ImageOperations Operations { get; }

    public abstract string Key { get; }
    public abstract string Title { get; }
    public abstract string Description { get; }

    public bool Matches(string input)
    {
        if (input.Equals(Key, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var alias in _aliases)
        {
            if (alias.Equals(input, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public abstract void Describe();
    public abstract void Execute();
}

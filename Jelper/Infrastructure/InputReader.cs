using System;

namespace Jelper.Infrastructure;

internal sealed class InputReader
{
    private readonly string _exitKeyword;

    public InputReader(string exitKeyword)
    {
        _exitKeyword = exitKeyword;
    }

    public string ExitKeyword => _exitKeyword;

    public string ReadRequiredInput(string prompt)
    {
        while (true)
        {
            Console.WriteLine($"{prompt} (or type '{_exitKeyword}' to cancel)");
            Console.Write("> ");

            var line = Console.ReadLine();
            if (line is null)
            {
                continue;
            }

            line = line.Trim();

            if (line.Equals(_exitKeyword, StringComparison.OrdinalIgnoreCase))
            {
                throw new UserExitRequestedException();
            }

            if (line.Length == 0)
            {
                Console.WriteLine("Input cannot be empty.");
                continue;
            }

            return line;
        }
    }

    public int ReadPositiveInt(string prompt, int minimum)
    {
        while (true)
        {
            var value = ReadRequiredInput(prompt);
            if (!int.TryParse(value, out var number))
            {
                Console.WriteLine("Value must be an integer.");
                continue;
            }

            if (number < minimum)
            {
                Console.WriteLine($"Value must be >= {minimum}.");
                continue;
            }

            return number;
        }
    }
}

using System;
using Jelper.Infrastructure;
using Jelper.Services;

namespace Jelper.Commands;

internal sealed class ConvertWebpCommand : ConsoleCommandBase
{
    public ConvertWebpCommand(InputReader input, ImageOperations operations)
        : base(input, operations, "webp2png", "webp-to-png", "convert-webp-to-png")
    {
    }

    public override string Key => "1";
    public override string Title => "webp2png";
    public override string Description => "Convert every WEBP file in the folder to PNG (progress shown).";

    public override void Describe()
    {
        Console.WriteLine();
        Console.WriteLine("You selected 1 - webp2png.");
        Console.WriteLine("This command scans the current folder for *.webp files, converts them to PNG, and shows progress as [current/total].");
        Console.WriteLine($"Type '{Input.ExitKeyword}' at any time to cancel and return to the legend.");
    }

    public override void Execute()
    {
        Console.WriteLine("Step 1: decide what to do with PNG files that already exist.");
        var overwrite = Input.ReadYesNo("Overwrite existing PNG files?");
        Console.WriteLine("Step 2: converting every WEBP file. Progress is shown as [current/total].");
        Operations.ConvertWebpToPng(overwrite);
    }
}

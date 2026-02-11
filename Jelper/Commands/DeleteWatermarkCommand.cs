using System;
using Jelper.Infrastructure;
using Jelper.Services;

namespace Jelper.Commands;

internal sealed class DeleteWatermarkCommand : ConsoleCommandBase
{
    public DeleteWatermarkCommand(InputReader input, ImageOperations operations)
        : base(input, operations, "delete-watermark", "watermark", "trim-watermark")
    {
    }

    public override string Key => "2";
    public override string Title => "delete-watermark";
    public override string Description => "Remove a bottom strip from each PNG while keeping ratio and center.";

    public override void Describe()
    {
        Console.WriteLine();
        Console.WriteLine("You selected 2 - delete-watermark.");
        Console.WriteLine("This command trims a fixed number of pixels from the bottom of every PNG, then crops equally from both sides to keep the center aligned and the aspect ratio untouched.");
        Console.WriteLine($"Type '{Input.ExitKeyword}' at any time to cancel and return to the legend.");
    }

    public override void Execute()
    {
        Console.WriteLine("Step 1: provide the integer number of pixels to cut from the bottom of each PNG.");
        var pixels = Input.ReadPositiveInt("Pixels to remove from the bottom (integer >= 1)", minimum: 1);
        Console.WriteLine("Step 2: cropping each PNG while keeping aspect ratio and center aligned horizontally.");
        Operations.RemoveWatermark(pixels);
    }
}

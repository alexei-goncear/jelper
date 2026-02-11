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
    public override string Description => "Trim a bottom strip from PNGs.";

    public override void Describe()
    {
        Console.WriteLine();
        Console.WriteLine("delete-watermark: cut pixels from the bottom and keep the center.");
    }

    public override void Execute()
    {
        Console.WriteLine("Pixels to trim from the bottom?");
        var pixels = Input.ReadPositiveInt("Pixels to remove (>= 1)", minimum: 1);
        Console.WriteLine("Cropping PNG files...");
        Operations.RemoveWatermark(pixels);
    }
}

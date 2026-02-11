using System;
using Jelper.Infrastructure;
using Jelper.Services;

namespace Jelper.Commands;

internal sealed class ResizeImageCommand : ConsoleCommandBase
{
    public ResizeImageCommand(InputReader input, ImageOperations operations)
        : base(input, operations, "resize", "resize-image", "png-resize")
    {
    }

    public override string Key => "3";
    public override string Title => "resize-image";
    public override string Description => "Stretch every PNG/JPG to the exact size.";

    public override void Describe()
    {
        Console.WriteLine();
        Console.WriteLine("resize-image: force all PNG/JPG files to the size you enter.");
    }

    public override void Execute()
    {
        Console.WriteLine("Enter target size (width first, then height).");
        var width = Input.ReadPositiveInt("Width in px (>= 1)", minimum: 1);
        var height = Input.ReadPositiveInt("Height in px (>= 1)", minimum: 1);
        Console.WriteLine("Resizing PNG/JPG files...");
        Operations.ResizeImages(width, height);
    }
}

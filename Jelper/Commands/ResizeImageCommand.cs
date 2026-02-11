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
    public override string Description => "Resize every PNG to the provided height and width.";

    public override void Describe()
    {
        Console.WriteLine();
        Console.WriteLine("You selected 3 - resize-image.");
        Console.WriteLine("This command resizes every PNG in the folder to the provided height and width values.");
        Console.WriteLine($"Type '{Input.ExitKeyword}' at any time to cancel and return to the legend.");
    }

    public override void Execute()
    {
        Console.WriteLine("Step 1: provide the new size (height first, then width). Only integers are accepted.");
        var height = Input.ReadPositiveInt("Target height in pixels (integer >= 1)", minimum: 1);
        var width = Input.ReadPositiveInt("Target width in pixels (integer >= 1)", minimum: 1);
        Console.WriteLine("Step 2: resizing each PNG. Progress is shown as [current/total].");
        Operations.ResizePng(width, height);
    }
}

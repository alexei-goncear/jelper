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
    public override string Description => "Convert WEBP files to PNG.";

    public override void Describe()
    {
        Console.WriteLine();
        Console.WriteLine("webp2png: convert every WEBP here to PNG.");
    }

    public override void Execute()
    {
        Console.WriteLine("Converting WEBP files...");
        Operations.ConvertWebpToPng();
    }
}

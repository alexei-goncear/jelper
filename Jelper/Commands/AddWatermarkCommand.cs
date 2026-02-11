using System;
using System.IO;
using System.Linq;
using Jelper.Infrastructure;
using Jelper.Services;

namespace Jelper.Commands;

internal sealed class AddWatermarkCommand : ConsoleCommandBase
{
    public AddWatermarkCommand(InputReader input, ImageOperations operations)
        : base(input, operations, "add-watermark", "watermark-add", "apply-watermark")
    {
    }

    public override string Key => "4";
    public override string Title => "add-watermark";
    public override string Description => "Resize PNG/JPG files and apply a watermark.";

    public override void Describe()
    {
        Console.WriteLine();
        Console.WriteLine("add-watermark: resize PNG/JPG files and overlay a selected watermark.");
    }

    public override void Execute()
    {
        Console.WriteLine("Enter target size (width first, then height).");
        var width = Input.ReadPositiveInt("Width in px (>= 1)", minimum: 1);
        var height = Input.ReadPositiveInt("Height in px (>= 1)", minimum: 1);
        Console.WriteLine("Looking for PNG watermarks in /images/watermarks...");
        var watermarkPath = SelectWatermark();
        if (watermarkPath is null)
        {
            Console.WriteLine("No watermarks available. Add PNG files to /images/watermarks and try again.");
            return;
        }

        var watermarkName = Path.GetFileName(watermarkPath);
        Console.WriteLine($"Resizing PNG/JPG files to {width}x{height} and applying {watermarkName}...");
        Operations.ResizeAndWatermark(width, height, watermarkPath);
    }

    private string? SelectWatermark()
    {
        var watermarksDirectory = GetWatermarksDirectory();
        var watermarkFiles = Directory
            .EnumerateFiles(watermarksDirectory, "*.png", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (watermarkFiles.Count == 0)
        {
            return null;
        }

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Available watermarks:");
            for (var i = 0; i < watermarkFiles.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {Path.GetFileName(watermarkFiles[i])}");
            }

            var selection = Input.ReadRequiredInput("Choose by number or file name");

            if (int.TryParse(selection, out var number) && number >= 1 && number <= watermarkFiles.Count)
            {
                return watermarkFiles[number - 1];
            }

            var matchedByName = watermarkFiles.FirstOrDefault(file =>
                Path.GetFileName(file).Equals(selection, StringComparison.OrdinalIgnoreCase));

            if (matchedByName is not null)
            {
                return matchedByName;
            }

            Console.WriteLine("Unknown watermark. Try again.");
        }
    }

    private static string GetWatermarksDirectory()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var imagesDirectory = Path.Combine(currentDirectory, "images");
        var watermarksDirectory = Path.Combine(imagesDirectory, "watermarks");
        Directory.CreateDirectory(watermarksDirectory);
        return watermarksDirectory;
    }
}

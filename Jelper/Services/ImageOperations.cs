using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageMagick;

namespace Jelper.Services;

internal sealed class ImageOperations
{
    public void ConvertWebpToPng(bool overwriteExisting)
    {
        var files = GetFiles("*.webp");
        if (files.Count == 0)
        {
            Console.WriteLine("No WEBP files were found in the current folder.");
            return;
        }

        var converted = 0;
        var skipped = 0;
        var total = files.Count;
        Console.WriteLine($"Found {total} WEBP file(s). Starting conversion...");

        for (var index = 0; index < total; index++)
        {
            var file = files[index];
            var destinationPath = Path.ChangeExtension(file, ".png");
            var progress = FormatProgress(index + 1, total);
            var fileName = Path.GetFileName(file);

            if (!overwriteExisting && File.Exists(destinationPath))
            {
                skipped++;
                Console.WriteLine($"{progress} Skipped {Path.GetFileName(destinationPath)} (already exists).");
                continue;
            }

            try
            {
                using var image = new MagickImage(file);
                image.Write(destinationPath, MagickFormat.Png);
                converted++;
                Console.WriteLine($"{progress} Created {Path.GetFileName(destinationPath)} from {fileName}.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{progress} Failed to convert {fileName}: {ex.Message}");
            }
        }

        Console.WriteLine($"webp2png finished. Converted {converted} of {total}, skipped {skipped}.");
    }

    public void RemoveWatermark(int pixelsToRemove)
    {
        var files = GetFiles("*.png");
        if (files.Count == 0)
        {
            Console.WriteLine("No PNG files were found in the current folder.");
            return;
        }

        var total = files.Count;
        Console.WriteLine($"Found {total} PNG file(s). Removing watermark...");

        for (var index = 0; index < total; index++)
        {
            var file = files[index];
            var progress = FormatProgress(index + 1, total);
            var fileName = Path.GetFileName(file);

            try
            {
                using var image = new MagickImage(file);
                if (image.Height <= pixelsToRemove)
                {
                    Console.Error.WriteLine($"{progress} Cannot trim {pixelsToRemove}px from {fileName} because the image height is {image.Height}px. Skipped.");
                    continue;
                }

                var originalWidth = image.Width;
                var originalHeight = image.Height;
                var targetHeight = originalHeight - pixelsToRemove;
                var targetWidth = (int)Math.Round(originalWidth * targetHeight / (double)originalHeight);
                targetWidth = Math.Clamp(targetWidth, 1, originalWidth);
                var widthReduction = originalWidth - targetWidth;
                var leftCrop = widthReduction / 2;

                var geometry = new MagickGeometry(leftCrop, 0, targetWidth, targetHeight)
                {
                    IgnoreAspectRatio = false
                };

                image.Crop(geometry);
                image.Write(file, MagickFormat.Png);
                Console.WriteLine($"{progress} Watermark removed from {fileName}.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{progress} Failed to update {fileName}: {ex.Message}");
            }
        }

        Console.WriteLine("Watermark removal finished for all PNG files.");
    }

    public void ResizePng(int targetWidth, int targetHeight)
    {
        var files = GetFiles("*.png");
        if (files.Count == 0)
        {
            Console.WriteLine("No PNG files were found in the current folder.");
            return;
        }

        var total = files.Count;
        Console.WriteLine($"Found {total} PNG file(s). Resizing...");

        for (var index = 0; index < total; index++)
        {
            var file = files[index];
            var progress = FormatProgress(index + 1, total);
            var fileName = Path.GetFileName(file);

            try
            {
                using var image = new MagickImage(file);
                image.Resize(targetWidth, targetHeight);
                image.Write(file, MagickFormat.Png);
                Console.WriteLine($"{progress} Resized {fileName} to {targetWidth}x{targetHeight}.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{progress} Failed to resize {fileName}: {ex.Message}");
            }
        }

        Console.WriteLine("Resize finished for all PNG files.");
    }

    private static List<string> GetFiles(string searchPattern)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        return Directory.EnumerateFiles(currentDirectory, searchPattern, SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string FormatProgress(int current, int total) => $"[{current}/{total}]";
}

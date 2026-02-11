using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageMagick;

namespace Jelper.Services;

internal sealed class ImageOperations
{
    private static readonly string[] SupportedImagePatterns = { "*.png", "*.jpg", "*.jpeg" };
    public void ConvertWebpToPng()
    {
        var files = GetFiles("*.webp");
        if (files.Count == 0)
        {
            Console.WriteLine("No WEBP files were found in /images.");
            return;
        }

        var converted = 0;
        var total = files.Count;
        Console.WriteLine($"Found {total} WEBP file(s) in /images. Starting conversion...");

        for (var index = 0; index < total; index++)
        {
            var file = files[index];
            var destinationPath = AppendOperationSuffix(Path.ChangeExtension(file, ".png"), "converted");
            var progress = FormatProgress(index + 1, total);
            var fileName = Path.GetFileName(file);

            try
            {
                var existed = File.Exists(destinationPath);
                using var image = new MagickImage(file);
                image.Write(destinationPath, MagickFormat.Png);
                converted++;
                var action = existed ? "Updated" : "Created";
                Console.WriteLine($"{progress} {action} {Path.GetFileName(destinationPath)} from {fileName}.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{progress} Failed to convert {fileName}: {ex.Message}");
            }
        }

        Console.WriteLine($"webp2png finished. Converted {converted} of {total}.");
    }

    public void RemoveWatermark(int pixelsToRemove)
    {
        var files = GetSupportedImageFiles();
        if (files.Count == 0)
        {
            Console.WriteLine("No PNG/JPG files were found in /images.");
            return;
        }

        var total = files.Count;
        Console.WriteLine($"Found {total} PNG/JPG file(s) in /images. Removing watermark...");

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

                var destinationPath = AppendOperationSuffix(file, "trimmed");
                image.Crop(geometry);
                var format = GetFormatForPath(destinationPath);
                image.Write(destinationPath, format);
                Console.WriteLine($"{progress} Saved {Path.GetFileName(destinationPath)} (trimmed {pixelsToRemove}px).");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{progress} Failed to update {fileName}: {ex.Message}");
            }
        }

        Console.WriteLine("Watermark removal finished for all PNG/JPG files.");
    }

    public void ResizeImages(int targetWidth, int targetHeight)
    {
        var files = GetSupportedImageFiles();
        if (files.Count == 0)
        {
            Console.WriteLine("No PNG/JPG files were found in /images.");
            return;
        }

        var total = files.Count;
        Console.WriteLine($"Found {total} PNG/JPG file(s) in /images. Resizing...");

        for (var index = 0; index < total; index++)
        {
            var file = files[index];
            var progress = FormatProgress(index + 1, total);
            var fileName = Path.GetFileName(file);

            try
            {
                using var image = new MagickImage(file);
                var geometry = new MagickGeometry(targetWidth, targetHeight)
                {
                    IgnoreAspectRatio = true
                };
                var destinationPath = AppendOperationSuffix(file, "resized");
                image.Resize(geometry);
                var format = GetFormatForPath(destinationPath);
                image.Write(destinationPath, format);
                Console.WriteLine($"{progress} Saved {Path.GetFileName(destinationPath)} ({targetWidth}x{targetHeight}).");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{progress} Failed to resize {fileName}: {ex.Message}");
            }
        }

        Console.WriteLine("Resize finished for all PNG/JPG files.");
    }

    public void ResizeAndWatermark(int targetWidth, int targetHeight, string watermarkPath)
    {
        if (!File.Exists(watermarkPath))
        {
            Console.WriteLine($"Watermark file {Path.GetFileName(watermarkPath)} was not found. Nothing to do.");
            return;
        }

        var files = GetSupportedImageFiles();
        if (files.Count == 0)
        {
            Console.WriteLine("No PNG/JPG files were found in /images.");
            return;
        }

        var watermarkFileName = Path.GetFileName(watermarkPath);
        Console.WriteLine($"Found {files.Count} PNG/JPG file(s) in /images. Preparing watermark {watermarkFileName}...");

        MagickImage watermark;
        try
        {
            watermark = new MagickImage(watermarkPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load watermark {watermarkFileName}: {ex.Message}");
            return;
        }

        using (watermark)
        {
            var total = files.Count;
            for (var index = 0; index < total; index++)
            {
                var file = files[index];
                var progress = FormatProgress(index + 1, total);
                var fileName = Path.GetFileName(file);

                try
                {
                    using var image = new MagickImage(file);
                    var geometry = new MagickGeometry(targetWidth, targetHeight)
                    {
                        IgnoreAspectRatio = true
                    };
                    image.Resize(geometry);

                    using var preparedWatermark = PrepareWatermarkFor(image, watermark);
                    var offsetX = Math.Max(0, image.Width - preparedWatermark.Width);
                    var offsetY = Math.Max(0, image.Height - preparedWatermark.Height);
                    image.Composite(preparedWatermark, offsetX, offsetY, CompositeOperator.Over);

                    var destinationPath = AppendOperationSuffix(file, "watermarked");
                    var format = GetFormatForPath(destinationPath);
                    image.Write(destinationPath, format);
                    Console.WriteLine($"{progress} Saved {Path.GetFileName(destinationPath)} ({targetWidth}x{targetHeight}, watermark: {watermarkFileName}).");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"{progress} Failed to watermark {fileName}: {ex.Message}");
                }
            }

            Console.WriteLine("Resize + watermark finished for all PNG/JPG files.");
        }
    }

    private static IMagickImage<ushort> PrepareWatermarkFor(IMagickImage<ushort> baseImage, IMagickImage<ushort> watermark)
    {
        var clone = watermark.Clone();
        if (clone.Width <= baseImage.Width && clone.Height <= baseImage.Height)
        {
            return clone;
        }

        var geometry = new MagickGeometry(baseImage.Width, baseImage.Height)
        {
            IgnoreAspectRatio = false
        };
        clone.Resize(geometry);
        return clone;
    }

    private static List<string> GetFiles(string searchPattern)
    {
        var imagesDirectory = GetImagesDirectory();
        return Directory.EnumerateFiles(imagesDirectory, searchPattern, SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> GetSupportedImageFiles()
    {
        var imagesDirectory = GetImagesDirectory();
        return SupportedImagePatterns
            .SelectMany(pattern => Directory.EnumerateFiles(imagesDirectory, pattern, SearchOption.TopDirectoryOnly))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static MagickFormat GetFormatForPath(string path)
    {
        var extension = Path.GetExtension(path);
        if (string.IsNullOrEmpty(extension))
        {
            return MagickFormat.Png;
        }

        return extension.ToLowerInvariant() switch
        {
            ".png" => MagickFormat.Png,
            ".jpg" => MagickFormat.Jpeg,
            ".jpeg" => MagickFormat.Jpeg,
            _ => MagickFormat.Png
        };
    }

    private static string FormatProgress(int current, int total) => $"[{current}/{total}]";

    private static string GetImagesDirectory()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var imagesDirectory = Path.Combine(currentDirectory, "images");
        Directory.CreateDirectory(imagesDirectory);
        return imagesDirectory;
    }

    private static string AppendOperationSuffix(string path, string operation)
    {
        var directory = Path.GetDirectoryName(path) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);
        var newName = $"{fileNameWithoutExtension}_{operation}{extension}";
        return Path.Combine(directory, newName);
    }
}

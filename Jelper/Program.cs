using Jelper.Commands;
using Jelper.Infrastructure;
using Jelper.Services;

namespace Jelper;

internal static class Program
{
    public static int Main(string[] args)
    {
        var inputReader = new InputReader("exit");
        var operations = new ImageOperations();

        var commands = new IConsoleCommand[]
        {
            new ConvertWebpCommand(inputReader, operations),
            new DeleteWatermarkCommand(inputReader, operations),
            new ResizeImageCommand(inputReader, operations)
        };

        var shell = new InteractiveShell(inputReader, commands);
        shell.Run();
        return 0;
    }
}

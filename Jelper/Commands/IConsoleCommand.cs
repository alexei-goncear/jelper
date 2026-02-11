namespace Jelper.Commands;

internal interface IConsoleCommand
{
    string Key { get; }
    string Title { get; }
    string Description { get; }
    bool Matches(string input);
    void Describe();
    void Execute();
}

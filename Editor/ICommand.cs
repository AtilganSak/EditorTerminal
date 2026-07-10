namespace EditorTerminal
{
    public interface ICommand
    {
        string Name { get; }

        string Execute(string[] args);
    }
}

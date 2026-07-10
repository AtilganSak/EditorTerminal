namespace EditorTerminal
{
    public class ClearCommand : ICommand, ICommandHelp
    {
        public string Name => "clear";

        public string Execute(string[] args)
        {
            return null;
        }

        public string GetHelp(string[] args)
        {
            return "clear - clears this terminal tab's history.";
        }
    }
}

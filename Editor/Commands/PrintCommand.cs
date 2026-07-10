namespace EditorTerminal
{
    public class PrintCommand : ICommand, ICommandHelp
    {
        public string Name => "print";

        public string Execute(string[] args)
        {
            return string.Join(" ", args);
        }

        public string GetHelp(string[] args)
        {
            return "print <text> - prints text to the terminal.\nExample: print \"Hello World\"";
        }
    }
}

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
            return "print <metin> - metni terminale yazdirir.\nOrnek: print \"Hello World\"";
        }
    }
}

using UnityEditor;

namespace EditorTerminal
{
    public class PlayCommand : ICommand, ICommandHelp
    {
        public string Name => "play";

        public string Execute(string[] args)
        {
            if (EditorApplication.isPlaying)
                return "Zaten Play Mode'da.";

            EditorApplication.isPlaying = true;
            return null;
        }

        public string GetHelp(string[] args)
        {
            return "play - Play Mode'a girer.";
        }
    }
}

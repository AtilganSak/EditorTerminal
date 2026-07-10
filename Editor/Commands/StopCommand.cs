using UnityEditor;

namespace EditorTerminal
{
    public class StopCommand : ICommand, ICommandHelp
    {
        public string Name => "stop";

        public string Execute(string[] args)
        {
            if (!EditorApplication.isPlaying)
                return "Zaten Play Mode'da degilsin.";

            EditorApplication.isPlaying = false;
            return null;
        }

        public string GetHelp(string[] args)
        {
            return "stop - exits Play Mode.";
        }
    }
}

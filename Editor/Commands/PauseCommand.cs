using UnityEditor;

namespace EditorTerminal
{
    public class PauseCommand : ICommand, ICommandHelp
    {
        public string Name => "pause";

        public string Execute(string[] args)
        {
            if (!EditorApplication.isPlaying)
                return "Play Mode'da degilsin.";

            EditorApplication.isPaused = !EditorApplication.isPaused;
            return EditorApplication.isPaused ? "Duraklatildi." : "Devam ediyor.";
        }

        public string GetHelp(string[] args)
        {
            return "pause - pauses while in Play Mode, resumes if run again.";
        }
    }
}

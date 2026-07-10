using UnityEditor;
using UnityEditor.SceneManagement;

namespace EditorTerminal
{
    public class QuitCommand : ICommand, ICommandHelp
    {
        public string Name => "quit";

        public string Execute(string[] args)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return "Iptal edildi.";

            EditorApplication.Exit(0);
            return null;
        }

        public string GetHelp(string[] args)
        {
            return "quit - Unity Editor'u kapatir. Kaydedilmemis degisiklik varsa once kaydetmeni sorar.";
        }
    }
}

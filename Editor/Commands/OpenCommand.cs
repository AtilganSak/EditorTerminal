using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace EditorTerminal
{
    public class OpenCommand : ICommand, ICommandArgSuggestions, ICommandHelp
    {
        private static readonly string[] _targets =
        {
            "projectSettings", "preferences", "lighting", "animation", "animator", "audioMixer",
            "navigation", "projectWindow", "console", "hierarchy", "inspector", "scene"
        };

        public string Name => "open";

        public IEnumerable<string> GetSuggestions(int argIndex, string[] argsBeforeCurrent)
        {
            if (argIndex == 0)
                return _targets;
            if (argIndex == 1 && argsBeforeCurrent.Length >= 1 && argsBeforeCurrent[0] == "scene")
                return SceneLookup.AllSceneNames();
            return null;
        }

        public string GetHelp(string[] args)
        {
            if (args.Length == 0)
            {
                return "open <target> [...] - opens the specified Unity window/setting.\n" +
                       "Targets: " + string.Join(", ", _targets) + "\n" +
                       "For details: open <target> -help";
            }

            switch (args[0])
            {
                case "projectSettings": return "open projectSettings\nOpens the Project Settings window.";
                case "preferences": return "open preferences\nOpens the Preferences (Editor settings) window.";
                case "lighting": return "open lighting\nOpens the Lighting window (Window/Rendering/Lighting).";
                case "animation": return "open animation\nOpens the Animation window.";
                case "animator": return "open animator\nOpens the Animator window.";
                case "audioMixer": return "open audioMixer\nOpens the Audio Mixer window.";
                case "navigation": return "open navigation\nOpens the Navigation (AI) window.";
                case "projectWindow": return "open projectWindow\nOpens the Project window.";
                case "console": return "open console\nOpens the Console window.";
                case "hierarchy": return "open hierarchy\nOpens the Hierarchy window.";
                case "inspector": return "open inspector\nOpens the Inspector window.";
                case "scene": return "open scene <name or path>\nOpens the given scene. If a name is given, it's searched for in the project.";
                default: return $"'{args[0]}' is not a valid target for open.";
            }
        }

        public string Execute(string[] args)
        {
            if (args.Length == 0)
                return "usage: open <target> ...";

            switch (args[0])
            {
                case "projectSettings": SettingsService.OpenProjectSettings(); return "Opened Project Settings";
                case "preferences": SettingsService.OpenUserPreferences(); return "Opened Preferences";
                case "lighting": return ExecuteMenu("Window/Rendering/Lighting");
                case "animation": return ExecuteMenu("Window/Animation/Animation");
                case "animator": return ExecuteMenu("Window/Animation/Animator");
                case "audioMixer": return ExecuteMenu("Window/Audio/Audio Mixer");
                case "navigation": return ExecuteMenu("Window/AI/Navigation");
                case "projectWindow": return ExecuteMenu("Window/General/Project");
                case "console": return ExecuteMenu("Window/General/Console");
                case "hierarchy": return ExecuteMenu("Window/General/Hierarchy");
                case "inspector": return ExecuteMenu("Window/General/Inspector");
                case "scene": return OpenScene(args);
                default: return $"'{args[0]}' is not a valid open target.";
            }
        }

        static string ExecuteMenu(string menuPath)
        {
            EditorApplication.ExecuteMenuItem(menuPath);
            return $"Opened {menuPath}";
        }

        static string OpenScene(string[] args)
        {
            if (args.Length < 2)
                return "usage: open scene <name or path>";

            var path = SceneLookup.ResolvePath(args[1]);
            if (path == null)
                return $"Scene '{args[1]}' not found.";

            EditorSceneManager.OpenScene(path);
            return $"Opened {path}";
        }
    }
}

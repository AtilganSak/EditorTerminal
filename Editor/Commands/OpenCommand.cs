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
                return "open <hedef> [...] - belirtilen Unity penceresini/ayarini acar.\n" +
                       "Hedefler: " + string.Join(", ", _targets) + "\n" +
                       "Detay icin: open <hedef> -help";
            }

            switch (args[0])
            {
                case "projectSettings": return "open projectSettings\nProject Settings penceresini acar.";
                case "preferences": return "open preferences\nPreferences (Editor ayarlari) penceresini acar.";
                case "lighting": return "open lighting\nLighting penceresini acar (Window/Rendering/Lighting).";
                case "animation": return "open animation\nAnimation penceresini acar.";
                case "animator": return "open animator\nAnimator penceresini acar.";
                case "audioMixer": return "open audioMixer\nAudio Mixer penceresini acar.";
                case "navigation": return "open navigation\nNavigation (AI) penceresini acar.";
                case "projectWindow": return "open projectWindow\nProject penceresini acar.";
                case "console": return "open console\nConsole penceresini acar.";
                case "hierarchy": return "open hierarchy\nHierarchy penceresini acar.";
                case "inspector": return "open inspector\nInspector penceresini acar.";
                case "scene": return "open scene <isim veya path>\nBelirtilen sahneyi acar. Isim verilirse projede aranir.";
                default: return $"'{args[0]}' open icin gecerli bir hedef degil.";
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

using UnityEditor;
using UnityEngine;

namespace EditorTerminal
{
    public static class TerminalTheme
    {
        private const string _fontSizeKey = "EditorTerminal.FontSize";
        private const string _fontColorKey = "EditorTerminal.FontColor";
        private const string _bgColorKey = "EditorTerminal.BgColor";

        public static int FontSize
        {
            get => EditorPrefs.GetInt(_fontSizeKey, 13);
            set => EditorPrefs.SetInt(_fontSizeKey, value);
        }

        public static Color FontColor
        {
            get => ParseColor(EditorPrefs.GetString(_fontColorKey, "#EBEBEB"), new Color(0.92f, 0.92f, 0.92f));
            set => EditorPrefs.SetString(_fontColorKey, "#" + ColorUtility.ToHtmlStringRGB(value));
        }

        public static Color BackgroundColor
        {
            get => ParseColor(EditorPrefs.GetString(_bgColorKey, "#000000"), Color.black);
            set => EditorPrefs.SetString(_bgColorKey, "#" + ColorUtility.ToHtmlStringRGB(value));
        }

        private static Color ParseColor(string hex, Color fallback)
        {
            return ColorUtility.TryParseHtmlString(hex, out var c) ? c : fallback;
        }

        public static void RebuildAllWindows()
        {
            EditorApplication.delayCall += () =>
            {
                foreach (var win in Resources.FindObjectsOfTypeAll<EditorTerminalWindow>())
                    win.Rebuild();
            };
        }
    }
}

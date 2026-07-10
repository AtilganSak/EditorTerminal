using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTerminal
{
    public static class WorkingDirectory
    {
        private const string _currentKey = "EditorTerminal.WorkingDirectory";
        private const string _defaultCurrent = "Assets";

        public static string Current
        {
            get => SessionState.GetString(_currentKey, _defaultCurrent);
            private set => SessionState.SetString(_currentKey, value);
        }

        public static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        public static string ToAbsolute(string assetRelativePath) =>
            Path.GetFullPath(Path.Combine(ProjectRoot, assetRelativePath));

        public static string Resolve(string input)
        {
            if (string.IsNullOrEmpty(input))
                return Current;

            var trimmed = input.Trim('/', '\\');
            var combined = trimmed.StartsWith("Assets", StringComparison.OrdinalIgnoreCase)
                ? trimmed
                : $"{Current}/{trimmed}";

            var full = Path.GetFullPath(Path.Combine(ProjectRoot, combined)).Replace('\\', '/').TrimEnd('/');
            var root = ProjectRoot.Replace('\\', '/').TrimEnd('/');

            if (full != root && !full.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase))
                return null;

            var relative = full.Length > root.Length ? full.Substring(root.Length + 1) : "";
            return relative.StartsWith("Assets", StringComparison.OrdinalIgnoreCase) ? relative : null;
        }

        public static void EnsureFolderExists(string assetFolderPath)
        {
            var full = ToAbsolute(assetFolderPath);
            if (!Directory.Exists(full))
            {
                Directory.CreateDirectory(full);
                AssetDatabase.Refresh();
            }
        }

        public static bool TrySetCurrent(string resolvedPath, out string error)
        {
            if (resolvedPath == null)
            {
                error = "Assets klasorunun disina cikilamaz.";
                return false;
            }

            if (!Directory.Exists(ToAbsolute(resolvedPath)))
            {
                error = $"'{resolvedPath}' bir klasor degil veya bulunamadi.";
                return false;
            }

            Current = resolvedPath;
            error = null;
            return true;
        }

        public static void ResetCurrent() => Current = _defaultCurrent;
    }
}

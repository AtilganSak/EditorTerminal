using System;
using System.IO;
using UnityEditor;

namespace EditorTerminal
{
    static class SceneLookup
    {
        public static string ResolvePath(string nameOrPath)
        {
            if (nameOrPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                return nameOrPath;

            var guids = AssetDatabase.FindAssets($"t:Scene {nameOrPath}", new[] {"Assets"});
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path).Equals(nameOrPath, StringComparison.OrdinalIgnoreCase))
                    return path;
            }
            return guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]) : null;
        }

        public static string[] AllSceneNames()
        {
            var guids = AssetDatabase.FindAssets("t:Scene", new[] {"Assets"});
            var names = new string[guids.Length];
            for (var i = 0; i < guids.Length; i++)
                names[i] = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guids[i]));
            return names;
        }
    }
}

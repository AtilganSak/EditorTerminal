using UnityEditor;

namespace EditorTerminal
{
    public static class AssetSelection
    {
        public static bool SelectAndPing(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (asset == null)
                return false;

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            return true;
        }
    }
}

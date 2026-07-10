using System;
using System.IO;
using UnityEditor;

namespace EditorTerminal
{
    public class ImportCommand : ICommand, ICommandHelp
    {
        public string Name => "import";

        public string Execute(string[] args)
        {
            if (args.Length < 1)
                return "usage: import <path>";

            var path = string.Join(" ", args);
            if (!File.Exists(path))
                return $"'{path}' bulunamadi.";

            if (!path.EndsWith(".unitypackage", StringComparison.OrdinalIgnoreCase))
                return "sadece .unitypackage dosyalari import edilebilir.";

            AssetDatabase.ImportPackage(path, interactive: true);
            return $"Importing {path}...";
        }

        public string GetHelp(string[] args)
        {
            return "import <path> - imports a .unitypackage file from your computer into the project (opens the Import Package window).\n" +
                   "Give a full path, e.g. import C:\\Users\\Admin\\Downloads\\MyPackage.unitypackage";
        }
    }
}

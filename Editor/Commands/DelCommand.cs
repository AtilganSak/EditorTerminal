using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace EditorTerminal
{
    public class DelCommand : ICommand, ICommandArgSuggestions, ICommandHelp
    {
        public string Name => "del";

        public IEnumerable<string> GetSuggestions(int argIndex, string[] argsBeforeCurrent)
        {
            return argIndex == 0 ? CurrentEntries() : null;
        }

        static IEnumerable<string> CurrentEntries()
        {
            var dir = WorkingDirectory.ToAbsolute(WorkingDirectory.Current);
            if (!Directory.Exists(dir))
                return System.Array.Empty<string>();

            var folders = Directory.GetDirectories(dir).Select(Path.GetFileName);
            var files = Directory.GetFiles(dir).Select(Path.GetFileName).Where(n => !n.EndsWith(".meta"));
            return folders.Concat(files);
        }

        public string Execute(string[] args)
        {
            if (args.Length < 1)
                return "usage: del <path>";

            var resolved = WorkingDirectory.Resolve(args[0]);
            if (resolved == null)
                return "Assets klasorunun disina cikilamaz.";

            if (!AssetDatabase.IsValidFolder(resolved) && !File.Exists(WorkingDirectory.ToAbsolute(resolved)))
                return $"'{resolved}' bulunamadi.";

            if (!AssetDatabase.DeleteAsset(resolved))
                return $"'{resolved}' silinemedi.";

            return $"Deleted {resolved}";
        }

        public string GetHelp(string[] args)
        {
            return "del <path> - Belirtilen dosyayi (uzantisiyla, orn: Scripts/Test.cs) siler. Mevcut dizine gore veya 'Assets/...' ile tam path verilebilir.";
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace EditorTerminal
{
    public class CopyCommand : ICommand, ICommandArgSuggestions, ICommandHelp
    {
        public string Name => "copy";

        public IEnumerable<string> GetSuggestions(int argIndex, string[] argsBeforeCurrent)
        {
            var dir = WorkingDirectory.ToAbsolute(WorkingDirectory.Current);
            if (!Directory.Exists(dir))
                return null;

            if (argIndex == 0)
            {
                var folders = Directory.GetDirectories(dir).Select(Path.GetFileName);
                var files = Directory.GetFiles(dir).Select(Path.GetFileName).Where(n => !n.EndsWith(".meta"));
                return folders.Concat(files);
            }

            if (argIndex == 1)
                return Directory.GetDirectories(dir).Select(Path.GetFileName);

            return null;
        }

        public string Execute(string[] args)
        {
            if (args.Length < 2)
                return "usage: copy <source> <target>";

            var source = WorkingDirectory.Resolve(args[0]);
            var target = WorkingDirectory.Resolve(args[1]);
            if (source == null || target == null)
                return "Assets klasorunun disina cikilamaz.";

            if (!AssetDatabase.IsValidFolder(source) && !File.Exists(WorkingDirectory.ToAbsolute(source)))
                return $"'{source}' bulunamadi.";

            var targetParent = Path.GetDirectoryName(target)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(targetParent))
                WorkingDirectory.EnsureFolderExists(targetParent);

            if (!AssetDatabase.CopyAsset(source, target))
                return $"'{source}' -> '{target}' kopyalanamadi.";

            return $"Copied {source} -> {target}";
        }

        public string GetHelp(string[] args)
        {
            return "copy <source> <target> - Bir dosyayi veya klasoru (uzantisiyla) baska bir path'e kopyalar.";
        }
    }
}

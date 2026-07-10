using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EditorTerminal
{
    public class SelectCommand : ICommand, ICommandArgSuggestions, ICommandHelp
    {
        public string Name => "select";

        public IEnumerable<string> GetSuggestions(int argIndex, string[] argsBeforeCurrent)
        {
            if (argIndex != 0)
                return null;

            var dir = WorkingDirectory.ToAbsolute(WorkingDirectory.Current);
            if (!Directory.Exists(dir))
                return null;

            var folders = Directory.GetDirectories(dir).Select(Path.GetFileName);
            var files = Directory.GetFiles(dir).Select(Path.GetFileName).Where(n => !n.EndsWith(".meta"));
            return folders.Concat(files);
        }

        public string Execute(string[] args)
        {
            if (args.Length < 1)
                return "usage: select <path>";

            var resolved = WorkingDirectory.Resolve(args[0]);
            if (resolved == null)
                return "Assets klasorunun disina cikilamaz.";

            if (!AssetSelection.SelectAndPing(resolved))
                return $"'{resolved}' bulunamadi.";

            return $"Selected {resolved}";
        }

        public string GetHelp(string[] args)
        {
            return "select <path> - Belirtilen dosya/klasoru Project penceresinde secip focuslar. Mevcut dizine gore veya 'Assets/...' ile tam path verilebilir.";
        }
    }
}

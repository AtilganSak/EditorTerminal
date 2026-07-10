using System.IO;
using System.Linq;

namespace EditorTerminal
{
    public class LsCommand : ICommand, ICommandHelp
    {
        public string Name => "ls";

        public string Execute(string[] args)
        {
            var dir = WorkingDirectory.ToAbsolute(WorkingDirectory.Current);
            if (!Directory.Exists(dir))
                return $"'{WorkingDirectory.Current}' bulunamadi.";

            var folders = Directory.GetDirectories(dir)
                .Select(Path.GetFileName)
                .OrderBy(n => n)
                .Select(n => $"{n}/");

            var files = Directory.GetFiles(dir)
                .Select(Path.GetFileName)
                .Where(n => !n.EndsWith(".meta"))
                .OrderBy(n => n);

            var entries = folders.Concat(files).ToList();
            return entries.Count > 0 ? string.Join("\n", entries) : "(bos)";
        }

        public string GetHelp(string[] args)
        {
            return "ls - lists the folders and files in the current directory. Folders end with '/'.";
        }
    }
}

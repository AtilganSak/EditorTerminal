using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EditorTerminal
{
    public class CdCommand : ICommand, ICommandArgSuggestions, ICommandHelp
    {
        public string Name => "cd";

        public IEnumerable<string> GetSuggestions(int argIndex, string[] argsBeforeCurrent)
        {
            if (argIndex != 0)
                return null;

            var dir = WorkingDirectory.ToAbsolute(WorkingDirectory.Current);
            if (!Directory.Exists(dir))
                return null;

            var folders = Directory.GetDirectories(dir).Select(Path.GetFileName);
            return new[] { "..", "-r" }.Concat(folders);
        }

        public string Execute(string[] args)
        {
            if (args.Length < 1)
                return "usage: cd <path>";

            if (string.Equals(args[0], "-r", System.StringComparison.OrdinalIgnoreCase))
            {
                WorkingDirectory.ResetCurrent();
                return null;
            }

            var resolved = WorkingDirectory.Resolve(args[0]);
            if (!WorkingDirectory.TrySetCurrent(resolved, out var error))
                return error;

            return null;
        }

        public string GetHelp(string[] args)
        {
            return "cd <path> - moves to the given folder. 'cd ..' goes up one folder. 'cd -r' resets the location to Assets. You can't leave Assets.";
        }
    }
}

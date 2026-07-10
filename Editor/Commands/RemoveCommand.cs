using System.Collections.Generic;
using UnityEditor.PackageManager;

namespace EditorTerminal
{
    public class RemoveCommand : ICommand, ICommandArgSuggestions, ICommandHelp
    {
        private static readonly string[] _categories = { "package" };

        public string Name => "remove";

        public IEnumerable<string> GetSuggestions(int argIndex, string[] argsBeforeCurrent)
        {
            return argIndex == 0 ? _categories : null;
        }

        public string Execute(string[] args)
        {
            if (args.Length < 1)
                return "usage: remove package <id>";

            switch (args[0])
            {
                case "package": return RemovePackage(args);
                default: return $"'{args[0]}' is not a valid remove category.";
            }
        }

        static string RemovePackage(string[] args)
        {
            if (args.Length < 2)
                return "usage: remove package <id>";

            var id = args[1];
            PackageOps.PollAndReport(Client.Remove(id), "remove package", id);
            return null;
        }

        public string GetHelp(string[] args)
        {
            if (args.Length == 0)
            {
                return "remove <package> <name> - removes a definition from the project.\n" +
                       "For details: remove package -help";
            }

            switch (args[0])
            {
                case "package":
                    return "remove package <id> - removes a package via the Unity Package Manager (e.g. com.unity.timeline). This is async; check the result with the 'log' command.";
                default:
                    return $"'{args[0]}' is not a valid category for remove.";
            }
        }
    }
}

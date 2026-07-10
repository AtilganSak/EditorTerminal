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
                return "remove <package> <isim> - projeden bir tanimi kaldirir.\n" +
                       "Detay icin: remove package -help";
            }

            switch (args[0])
            {
                case "package":
                    return "remove package <id> - Unity Package Manager'dan bir paketi kaldirir (orn: com.unity.timeline). Islem asenkron; sonucu 'log' komutuyla kontrol et.";
                default:
                    return $"'{args[0]}' remove icin gecerli bir kategori degil.";
            }
        }
    }
}

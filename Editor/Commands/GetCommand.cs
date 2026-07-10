using System.Collections.Generic;
using System.Linq;

namespace EditorTerminal
{
    public class GetCommand : ICommand, ICommandArgSuggestions, ICommandHelp
    {
        private readonly SetCommand _settings = new SetCommand();

        public string Name => "get";

        public IEnumerable<string> GetSuggestions(int argIndex, string[] argsBeforeCurrent)
        {
            if (argIndex == 0)
                return _settings.Categories;

            if (argIndex == 1 && argsBeforeCurrent.Length >= 1)
                return _settings.KeysFor(argsBeforeCurrent[0]).Select(k => k.Key);

            return null;
        }

        public string Execute(string[] args)
        {
            if (args.Length < 2)
                return "usage: get <category> <key>";

            var (found, _, value) = _settings.ReadValue(args[0], args[1]);
            if (!found)
                return $"'{args[0]} {args[1]}' is not a recognized setting.";

            return value;
        }

        public string GetHelp(string[] args)
        {
            if (args.Length == 0)
            {
                var categories = _settings.Categories;
                return "get <kategori> <key> - bir Unity/Editor ayarinin guncel degerini okur.\n" +
                       "Kategoriler: " + string.Join(", ", categories) + "\n" +
                       "Detay icin: get <kategori> -help  veya  get <kategori> <key> -help";
            }

            if (args.Length == 1)
            {
                var keys = _settings.KeysFor(args[0]).ToList();
                if (keys.Count == 0)
                    return $"'{args[0]}' gecerli bir kategori degil.";

                var lines = keys.Select(k => $"  {k.Key} - {k.Description}");
                return $"get {args[0]} <key>\n" + string.Join("\n", lines);
            }

            var (found, description, _) = _settings.ReadValue(args[0], args[1]);
            if (!found)
                return $"'{args[0]} {args[1]}' gecerli bir ayar degil.";

            return $"get {args[0]} {args[1]}\n{description}";
        }
    }
}

using System.Linq;
using UnityEngine;

namespace EditorTerminal
{
    public class LogCommand : ICommand, ICommandHelp
    {
        private const int _defaultCount = 20;

        public string Name => "log";

        public string Execute(string[] args)
        {
            var count = _defaultCount;
            if (args.Length > 0 && !int.TryParse(args[0], out count))
                return $"'{args[0]}' gecerli bir sayi degil.";

            var entries = LogBuffer.Recent(count).ToList();
            if (entries.Count == 0)
                return "(henuz log yok - bu terminal acildiktan sonraki loglar burada gorunur)";

            return string.Join("\n", entries.Select(e => $"[{Tag(e.Type)}] {e.Message}"));
        }

        static string Tag(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    return "ERROR";
                case LogType.Warning:
                    return "WARN";
                default:
                    return "LOG";
            }
        }

        public string GetHelp(string[] args)
        {
            return "log [n] - Konsola gelen son n log/warning/error mesajini yazdirir (varsayilan 20). Sadece bu terminal acildiktan sonraki mesajlari yakalar.";
        }
    }
}

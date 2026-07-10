using System.Text.RegularExpressions;

namespace EditorTerminal
{
    public static class CommandParser
    {
        private static readonly Regex _tokenPattern = new Regex("\"([^\"]*)\"|(\\S+)", RegexOptions.Compiled);

        public static string[] Tokenize(string line)
        {
            var matches = _tokenPattern.Matches(line);
            var tokens = new string[matches.Count];
            for (var i = 0; i < matches.Count; i++)
            {
                var m = matches[i];
                tokens[i] = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
            }
            return tokens;
        }
    }
}

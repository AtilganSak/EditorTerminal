using System.Collections.Generic;

namespace EditorTerminal
{
    public interface ICommandArgSuggestions
    {
        IEnumerable<string> GetSuggestions(int argIndex, string[] argsBeforeCurrent);
    }
}

using System;
using System.Collections.Generic;

namespace EditorTerminal
{
    [Serializable]
    public class TerminalLine
    {
        public string Text;
        public bool IsCommand;
        public string Prompt;
    }

    [Serializable]
    public class TerminalSessionData
    {
        public string Title;
        public List<TerminalLine> Lines = new List<TerminalLine>();
        public List<string> History = new List<string>();
    }
}

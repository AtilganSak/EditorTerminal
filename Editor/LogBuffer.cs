using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorTerminal
{
    [InitializeOnLoad]
    public static class LogBuffer
    {
        private const int _capacity = 200;
        private static readonly LinkedList<(string Message, LogType Type)> _entries = new LinkedList<(string, LogType)>();

        static LogBuffer()
        {
            Application.logMessageReceived += (message, stackTrace, type) =>
            {
                _entries.AddLast((message, type));
                if (_entries.Count > _capacity)
                    _entries.RemoveFirst();
            };
        }

        public static IEnumerable<(string Message, LogType Type)> Recent(int count)
        {
            var skip = Mathf.Max(0, _entries.Count - count);
            var i = 0;
            foreach (var entry in _entries)
            {
                if (i++ >= skip)
                    yield return entry;
            }
        }
    }
}

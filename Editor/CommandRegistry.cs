using System;
using System.Collections.Generic;
using System.Linq;

namespace EditorTerminal
{
    public static class CommandRegistry
    {
        private static Dictionary<string, ICommand> _commands;

        private static Dictionary<string, ICommand> Commands
        {
            get
            {
                if (_commands == null)
                    _commands = DiscoverCommands();
                return _commands;
            }
        }

        // Scans all loaded assemblies for ICommand implementations and maps them by name.
        private static Dictionary<string, ICommand> DiscoverCommands()
        {
            var map = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase);

            var commandTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => SafeGetTypes(a))
                .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in commandTypes)
            {
                if (Activator.CreateInstance(type) is ICommand command)
                    map[command.Name] = command;
            }

            return map;
        }

        private static IEnumerable<Type> SafeGetTypes(System.Reflection.Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (System.Reflection.ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
        }

        public static void Warmup()
        {
            _ = Commands;
        }

        public static bool TryGetCommand(string name, out ICommand command)
        {
            return Commands.TryGetValue(name, out command);
        }

        public static IReadOnlyList<string> GetCommandNames()
        {
            return Commands.Values.Select(c => c.Name).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static string Execute(string line)
        {
            var tokens = CommandParser.Tokenize(line);
            if (tokens.Length == 0)
                return null;

            if (tokens.Length == 1 && string.Equals(tokens[0], "-help", StringComparison.OrdinalIgnoreCase))
                return ListAllCommands();

            var name = tokens[0];

            if (string.Equals(tokens[tokens.Length - 1], "-help", StringComparison.OrdinalIgnoreCase))
                return GetHelp(name, tokens);

            var args = tokens.Skip(1).ToArray();

            if (!Commands.TryGetValue(name, out var command))
                return $"'{name}' is not recognized as a command.";

            try
            {
                return command.Execute(args);
            }
            catch (Exception e)
            {
                return $"error: {e.Message}";
            }
        }

        private static string ListAllCommands()
        {
            var lines = GetCommandNames().Select(n =>
            {
                TryGetCommand(n, out var command);
                return command is ICommandHelp help ? help.GetHelp(Array.Empty<string>()).Split('\n')[0] : n;
            });
            return string.Join("\n", lines);
        }

        private static string GetHelp(string name, string[] tokens)
        {
            if (!Commands.TryGetValue(name, out var command))
                return $"'{name}' is not recognized as a command.";

            if (!(command is ICommandHelp helpProvider))
                return $"{command.Name}: no help defined.";

            var helpArgs = new string[tokens.Length - 2];
            Array.Copy(tokens, 1, helpArgs, 0, helpArgs.Length);
            return helpProvider.GetHelp(helpArgs);
        }
    }
}

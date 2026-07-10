using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EditorTerminal
{
    public class CreateCommand : ICommand, ICommandArgSuggestions, ICommandHelp
    {
        private static readonly string[] _subCommands =
            { "mono", "editor", "editorWindow", "so", "class", "struct", "material", "scene", "folder" };

        private static readonly string[] _commonColorNames =
            { "white", "black", "red", "green", "blue", "yellow", "cyan", "magenta", "gray", "orange" };

        public string Name => "create";

        public IEnumerable<string> GetSuggestions(int argIndex, string[] argsBeforeCurrent)
        {
            if (argIndex == 0)
                return _subCommands;

            if (argIndex == 2 && argsBeforeCurrent.Length >= 1 && argsBeforeCurrent[0] == "material")
                return _commonColorNames;

            return null;
        }

        public string GetHelp(string[] args)
        {
            if (args.Length == 0)
            {
                return "create <mono|editor|editorWindow|so|class|struct|material|scene> ... - creates a new file/asset under Assets.\n" +
                       "For details: create <sub-command> -help";
            }

            switch (args[0])
            {
                case "mono":
                    return "create mono <name> [path]\nCreates an empty MonoBehaviour script. Writes to the current directory (terminal cwd) if path is omitted.";
                case "editor":
                    return "create editor <name> <target script> [path]\nCreates a custom Inspector (Editor) script for the target script ([CustomEditor]). Writes to the current directory (terminal cwd) if path is omitted.";
                case "editorWindow":
                    return "create editorWindow <name> [path]\nCreates an empty EditorWindow script, opened via a MenuItem. Writes to the current directory (terminal cwd) if path is omitted.";
                case "so":
                    return "create so <name> [file name] [menu name] [path]\nCreates a ScriptableObject script ([CreateAssetMenu]). Uses name for file name/menu name if omitted, writes to the current directory (terminal cwd) if path is omitted.";
                case "class":
                    return "create class <name> [path]\nCreates an empty C# class file with no Unity dependency. Writes to the current directory (terminal cwd) if path is omitted.";
                case "struct":
                    return "create struct <name> [path]\nCreates an empty C# struct file. Writes to the current directory (terminal cwd) if path is omitted.";
                case "material":
                    return "create material <name> [color] [path]\nCreates a new Material asset (URP Lit shader, falling back to Standard). color as hex (#RRGGBB) or color name (e.g. red); the shader's default color is used if omitted. Writes to the current directory (terminal cwd) if path is omitted.";
                case "scene":
                    return "create scene <name> [path]\nCreates and saves a new, empty scene. Writes to the current directory (terminal cwd) if path is omitted.";
                case "folder":
                    return "create folder <name> [path]\nCreates a new empty folder. Created under the current directory (terminal cwd) if path is omitted.";
                default:
                    return $"'{args[0]}' is not a valid sub-command for create.";
            }
        }

        public string Execute(string[] args)
        {
            if (args.Length == 0)
                return "usage: create <mono|editor|editorWindow|so|class|struct|material|scene> ...";

            var rest = new string[args.Length - 1];
            System.Array.Copy(args, 1, rest, 0, rest.Length);

            switch (args[0])
            {
                case "mono": return CreateMono(rest);
                case "editor": return CreateEditorScript(rest);
                case "editorWindow": return CreateEditorWindow(rest);
                case "so": return CreateScriptableObject(rest);
                case "class": return CreateClass(rest);
                case "struct": return CreateStruct(rest);
                case "material": return CreateMaterial(rest);
                case "scene": return CreateScene(rest);
                case "folder": return CreateFolder(rest);
                default: return $"'{args[0]}' is not a valid create target.";
            }
        }

        static string CreateMono(string[] args)
        {
            if (args.Length < 1)
                return "usage: create mono <name> [path]";

            var name = args[0];
            var content = $"using UnityEngine;\n\npublic class {name} : MonoBehaviour\n{{\n}}\n";
            return WriteTextAsset(ResolvePath(args, 1, $"{name}.cs"), content);
        }

        static string CreateEditorScript(string[] args)
        {
            if (args.Length < 2)
                return "usage: create editor <name> <target script> [path]";

            var name = args[0];
            var target = args[1];
            var content =
                "using UnityEditor;\n\n" +
                $"[CustomEditor(typeof({target}))]\n" +
                $"public class {name} : Editor\n" +
                "{\n" +
                "    public override void OnInspectorGUI()\n" +
                "    {\n" +
                "        DrawDefaultInspector();\n" +
                "    }\n" +
                "}\n";

            return WriteTextAsset(ResolvePath(args, 2, $"{name}.cs"), content);
        }

        static string CreateEditorWindow(string[] args)
        {
            if (args.Length < 1)
                return "usage: create editorWindow <name> [path]";

            var name = args[0];
            var content =
                "using UnityEditor;\nusing UnityEngine;\n\n" +
                $"public class {name} : EditorWindow\n" +
                "{\n" +
                $"    [MenuItem(\"Window/{name}\")]\n" +
                "    public static void Open()\n" +
                "    {\n" +
                $"        GetWindow<{name}>();\n" +
                "    }\n\n" +
                "    void OnGUI()\n" +
                "    {\n" +
                "    }\n" +
                "}\n";

            return WriteTextAsset(ResolvePath(args, 1, $"{name}.cs"), content);
        }

        static string CreateScriptableObject(string[] args)
        {
            if (args.Length < 1)
                return "usage: create so <name> [file name] [menu name] [path]";

            var name = args[0];
            var fileName = args.Length > 1 ? args[1] : name;
            var menuName = args.Length > 2 ? args[2] : name;
            var content =
                "using UnityEngine;\n\n" +
                $"[CreateAssetMenu(fileName = \"{fileName}\", menuName = \"{menuName}\")]\n" +
                $"public class {name} : ScriptableObject\n" +
                "{\n}\n";

            return WriteTextAsset(ResolvePath(args, 3, $"{name}.cs"), content);
        }

        static string CreateClass(string[] args)
        {
            if (args.Length < 1)
                return "usage: create class <name> [path]";

            var name = args[0];
            var content = $"public class {name}\n{{\n}}\n";
            return WriteTextAsset(ResolvePath(args, 1, $"{name}.cs"), content);
        }

        static string CreateStruct(string[] args)
        {
            if (args.Length < 1)
                return "usage: create struct <name> [path]";

            var name = args[0];
            var content = $"public struct {name}\n{{\n}}\n";
            return WriteTextAsset(ResolvePath(args, 1, $"{name}.cs"), content);
        }

        static string CreateMaterial(string[] args)
        {
            if (args.Length < 1)
                return "usage: create material <name> [color] [path]";

            var folder = args.Length > 2 ? args[2].TrimEnd('/') : WorkingDirectory.Current;
            var path = $"{folder}/{args[0]}.mat";

            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
                return $"'{path}' already exists.";

            Color? color = null;
            if (args.Length > 1)
            {
                if (!ColorUtility.TryParseHtmlString(args[1], out var parsed))
                    return $"'{args[1]}' gecerli bir renk degil. Ornek: #FF0000 veya red.";
                color = parsed;
            }

            EnsureFolder(folder);
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader);
            if (color.HasValue)
                material.color = color.Value;
            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.SaveAssets();
            AssetSelection.SelectAndPing(path);
            return $"Created {path}";
        }

        static string CreateScene(string[] args)
        {
            if (args.Length < 1)
                return "usage: create scene <name> [path]";

            var folder = args.Length > 1 ? args[1].TrimEnd('/') : WorkingDirectory.Current;
            var path = $"{folder}/{args[0]}.unity";

            if (File.Exists(Path.Combine(Application.dataPath, "..", path)))
                return $"'{path}' already exists.";

            EnsureFolder(folder);

            var previousActive = EditorSceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.CloseScene(scene, true);
            if (previousActive.IsValid())
                EditorSceneManager.SetActiveScene(previousActive);

            AssetSelection.SelectAndPing(path);
            return $"Created {path}";
        }

        static string CreateFolder(string[] args)
        {
            if (args.Length < 1)
                return "usage: create folder <name> [path]";

            var parent = args.Length > 1 ? args[1].TrimEnd('/') : WorkingDirectory.Current;
            var path = $"{parent}/{args[0]}";

            if (AssetDatabase.IsValidFolder(path))
                return $"'{path}' already exists.";

            EnsureFolder(path);
            AssetSelection.SelectAndPing(path);
            return $"Created {path}";
        }

        static string ResolvePath(string[] args, int pathArgIndex, string fileName)
        {
            var folder = args.Length > pathArgIndex ? args[pathArgIndex].TrimEnd('/') : WorkingDirectory.Current;
            return $"{folder}/{fileName}";
        }

        static void EnsureFolder(string assetFolderPath)
        {
            var full = Path.Combine(Application.dataPath, "..", assetFolderPath);
            if (!Directory.Exists(full))
            {
                Directory.CreateDirectory(full);
                AssetDatabase.Refresh();
            }
        }

        static string WriteTextAsset(string relativePath, string content)
        {
            var fullPath = Path.Combine(Application.dataPath, "..", relativePath);

            if (File.Exists(fullPath))
                return $"'{relativePath}' already exists.";

            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(fullPath, content);
            AssetDatabase.Refresh();
            AssetSelection.SelectAndPing(relativePath);
            return $"Created {relativePath}";
        }
    }
}

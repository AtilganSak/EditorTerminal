using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditorInternal;

namespace EditorTerminal
{
    public class AddCommand : ICommand, ICommandArgSuggestions, ICommandHelp
    {
        private static readonly string[] _categories = { "tag", "layer", "sortingLayer", "SDS", "scene", "package" };

        public string Name => "add";

        public IEnumerable<string> GetSuggestions(int argIndex, string[] argsBeforeCurrent)
        {
            if (argIndex == 0)
                return _categories;
            if (argIndex == 1 && argsBeforeCurrent.Length >= 1 && argsBeforeCurrent[0] == "scene")
                return SceneLookup.AllSceneNames();
            return null;
        }

        public string GetHelp(string[] args)
        {
            if (args.Length == 0)
            {
                return "add <tag|layer|sortingLayer|SDS|scene|package> <isim> - projeye yeni bir tanim ekler.\n" +
                       "Detay icin: add <kategori> -help";
            }

            switch (args[0])
            {
                case "tag":
                    return "add tag <isim>\nProjeye yeni bir Tag ekler (Tags & Layers).";
                case "layer":
                    return "add layer <isim>\nProjeye yeni bir Layer ekler, ilk bos slotu kullanir (index 8-31).";
                case "sortingLayer":
                    return "add sortingLayer <isim>\nProjeye yeni bir Sorting Layer ekler.\n" +
                           "Not: bu ayar icin public API yok, TagManager.asset dogrudan duzenleniyor - sirayi Tags & Layers penceresinden dogrula.";
                case "SDS":
                    return "add SDS <sembol>\nAktif build target icin Scripting Define Symbol ekler.";
                case "scene":
                    return "add scene <isim veya path>\nBelirtilen sahneyi Build Settings sahne listesine ekler.";
                case "package":
                    return "add package <id>\nUnity Package Manager uzerinden paket ekler (orn: com.unity.timeline veya bir git URL'i).\n" +
                           "Islem asenkron; sonucu 'log' komutuyla kontrol et.";
                default:
                    return $"'{args[0]}' add icin gecerli bir kategori degil.";
            }
        }

        public string Execute(string[] args)
        {
            if (args.Length < 1)
                return "usage: add <tag|layer|sortingLayer|SDS|scene|package> <name>";

            var rest = new string[args.Length - 1];
            Array.Copy(args, 1, rest, 0, rest.Length);

            switch (args[0])
            {
                case "tag": return AddTag(rest);
                case "layer": return AddLayer(rest);
                case "sortingLayer": return AddSortingLayer(rest);
                case "SDS": return AddDefineSymbol(rest);
                case "scene": return AddSceneToBuild(rest);
                case "package": return AddPackage(rest);
                default: return $"'{args[0]}' is not a valid add category.";
            }
        }

        static string AddTag(string[] args)
        {
            if (args.Length < 1)
                return "usage: add tag <name>";

            var name = args[0];
            if (InternalEditorUtility.tags.Contains(name))
                return $"Tag '{name}' already exists.";

            InternalEditorUtility.AddTag(name);
            return $"Added tag '{name}'";
        }

        static string AddLayer(string[] args)
        {
            if (args.Length < 1)
                return "usage: add layer <name>";

            var name = args[0];
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");

            for (var i = 8; i < layers.arraySize; i++)
            {
                var element = layers.GetArrayElementAtIndex(i);
                if (element.stringValue == name)
                    return $"Layer '{name}' already exists.";

                if (string.IsNullOrEmpty(element.stringValue))
                {
                    element.stringValue = name;
                    tagManager.ApplyModifiedProperties();
                    return $"Added layer '{name}' at index {i}";
                }
            }

            return "No free layer slots (max 32 layers).";
        }

        static string AddSortingLayer(string[] args)
        {
            if (args.Length < 1)
                return "usage: add sortingLayer <name>";

            var name = args[0];
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var sortingLayers = tagManager.FindProperty("m_SortingLayers");

            for (var i = 0; i < sortingLayers.arraySize; i++)
            {
                var nameProp = sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name");
                if (nameProp != null && nameProp.stringValue == name)
                    return $"Sorting layer '{name}' already exists.";
            }

            sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
            var newEntry = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
            newEntry.FindPropertyRelative("name").stringValue = name;
            tagManager.ApplyModifiedProperties();
            return $"Added sorting layer '{name}' (verify order in Tags & Layers).";
        }

        static string AddDefineSymbol(string[] args)
        {
            if (args.Length < 1)
                return "usage: add SDS <symbol>";

            var symbol = args[0];
            var target = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            var symbols = PlayerSettings.GetScriptingDefineSymbols(target)
                .Split(';')
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            if (symbols.Contains(symbol))
                return $"'{symbol}' is already defined.";

            symbols.Add(symbol);
            PlayerSettings.SetScriptingDefineSymbols(target, symbols.ToArray());
            return $"Added define symbol '{symbol}'";
        }

        static string AddSceneToBuild(string[] args)
        {
            if (args.Length < 1)
                return "usage: add scene <name or path>";

            var path = SceneLookup.ResolvePath(args[0]);
            if (path == null)
                return $"Scene '{args[0]}' not found.";

            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == path))
                return $"'{path}' is already in Build Settings.";

            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            return $"Added {path} to Build Settings";
        }

        static string AddPackage(string[] args)
        {
            if (args.Length < 1)
                return "usage: add package <id>";

            var id = args[0];
            PackageOps.PollAndReport(Client.Add(id), "add package", id);
            return null;
        }
    }
}

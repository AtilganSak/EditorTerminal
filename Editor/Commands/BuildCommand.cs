using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace EditorTerminal
{
    public class BuildCommand : ICommand, ICommandArgSuggestions, ICommandHelp
    {
        private static readonly string[] _boolValues = { "true", "false" };

        public string Name => "build";

        public IEnumerable<string> GetSuggestions(int argIndex, string[] argsBeforeCurrent)
        {
            return argIndex == 1 ? _boolValues : null;
        }

        public string Execute(string[] args)
        {
            if (args.Length < 1)
                return "usage: build <output klasoru> [true|false]";

            var buildAndRun = false;
            if (args.Length > 1 && !bool.TryParse(args[1], out buildAndRun))
                return $"'{args[1]}' true/false olmali.";

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
                return "Build Settings'te aktif sahne yok (add scene <isim> ile ekle).";

            var target = EditorUserBuildSettings.activeBuildTarget;
            var outputPath = ResolveOutputPath(args[0], target);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = target,
                options = buildAndRun ? BuildOptions.AutoRunPlayer : BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
                return $"error: build basarisiz ({summary.result}), {summary.totalErrors} hata.";

            var suffix = buildAndRun ? " - calistiriliyor" : "";
            return $"Build tamamlandi: {outputPath} ({summary.totalSize} byte, {summary.totalTime.TotalSeconds:F1}s){suffix}";
        }

        static string ResolveOutputPath(string folder, BuildTarget target)
        {
            var fileName = PlayerSettings.productName;
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    fileName += ".exe";
                    break;
                case BuildTarget.StandaloneOSX:
                    fileName += ".app";
                    break;
                case BuildTarget.StandaloneLinux64:
                    break;
                default:
                    // Android/iOS/WebGL vb. locationPathName'i klasor olarak bekler.
                    return folder;
            }

            return Path.Combine(folder, fileName);
        }

        public string GetHelp(string[] args)
        {
            return "build <output klasoru> [true|false] - Aktif build target icin, Build Settings'teki aktif sahnelerle projeyi derler.\n" +
                   "Dosya adi olarak Project Settings > Product Name kullanilir (Windows/Mac/Linux standalone icin uzanti otomatik eklenir).\n" +
                   "Ikinci parametre true ise build sonrasi calistirir (Build And Run). Ornek: build C:\\Builds\\MyGame true";
        }
    }
}

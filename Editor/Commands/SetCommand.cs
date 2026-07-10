using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace EditorTerminal
{
    public class SetCommand : ICommand, ICommandArgSuggestions, ICommandHelp
    {
        class SettingDef
        {
            public string Category;
            public string Key;
            public string Description;
            public string DefaultValue;
            public Func<int, string[], IEnumerable<string>> ValueSuggestions;
            public Func<string[], string> Apply;
            public Func<string> Getter;
        }

        private static readonly string[] _trueFalse = { "true", "false" };
        private static readonly string[] _commonColorNames =
            { "white", "black", "red", "green", "blue", "yellow", "cyan", "magenta", "gray", "orange" };

        static NamedBuildTarget ActiveTarget =>
            NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

        private readonly List<SettingDef> _defs;

        public SetCommand()
        {
            _defs = new List<SettingDef>
            {
                Def1("player", "colorSpace", "Renk uzayi (PlayerSettings.colorSpace).", () => new[] { "Gamma", "Linear" }, v =>
                {
                    PlayerSettings.colorSpace = (ColorSpace)Enum.Parse(typeof(ColorSpace), v);
                    return null;
                }, () => PlayerSettings.colorSpace.ToString(), defaultValue: "Gamma"),
                Def1("player", "graphicsJob", "Graphics Jobs acik/kapali (PlayerSettings.graphicsJobs).", () => _trueFalse, v =>
                {
                    PlayerSettings.graphicsJobs = Bool(v);
                    return null;
                }, () => BoolStr(PlayerSettings.graphicsJobs), defaultValue: "false"),
                Def1("player", "scriptingBackend", "Aktif build target icin scripting backend (Mono/IL2CPP).", () => new[] { "Mono", "IL2CPP" }, v =>
                {
                    PlayerSettings.SetScriptingBackend(ActiveTarget, v == "IL2CPP" ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
                    return null;
                }, () => PlayerSettings.GetScriptingBackend(ActiveTarget) == ScriptingImplementation.IL2CPP ? "IL2CPP" : "Mono", defaultValue: "Mono"),
                Def1("player", "apicoml", "Aktif build target icin API Compatibility Level. Degerler kurulu Unity'den dinamik cekilir.", () => Enum.GetNames(typeof(ApiCompatibilityLevel)), v =>
                {
                    PlayerSettings.SetApiCompatibilityLevel(ActiveTarget, (ApiCompatibilityLevel)Enum.Parse(typeof(ApiCompatibilityLevel), v));
                    return null;
                }, () => PlayerSettings.GetApiCompatibilityLevel(ActiveTarget).ToString()),
                Def1("player", "incGC", "Incremental Garbage Collector acik/kapali (PlayerSettings.gcIncremental).", () => _trueFalse, v =>
                {
                    PlayerSettings.gcIncremental = Bool(v);
                    return null;
                }, () => BoolStr(PlayerSettings.gcIncremental), defaultValue: "true"),
                Def1("player", "allowUC", "Unsafe code'a izin ver (PlayerSettings.allowUnsafeCode).", () => _trueFalse, v =>
                {
                    PlayerSettings.allowUnsafeCode = Bool(v);
                    return null;
                }, () => BoolStr(PlayerSettings.allowUnsafeCode), defaultValue: "false"),
                Def1("player", "packageName", "Android icin uygulama identifier'i (PlayerSettings.SetApplicationIdentifier, sadece Android).", null, v =>
                {
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, v);
                    return null;
                }, () => PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android)),
                Def1("player", "version", "Uygulama versiyonu (PlayerSettings.bundleVersion).", null, v =>
                {
                    PlayerSettings.bundleVersion = v;
                    return null;
                }, () => PlayerSettings.bundleVersion),
                Def1("player", "bundleVersionCode", "Android Bundle Version Code (PlayerSettings.Android.bundleVersionCode).", null, v =>
                {
                    if (!int.TryParse(v, out var code))
                        return $"'{v}' gecerli bir sayi degil.";
                    PlayerSettings.Android.bundleVersionCode = code;
                    return null;
                }, () => PlayerSettings.Android.bundleVersionCode.ToString()),
                Def1("player", "minAPI", "Android minimum SDK versiyonu. Degerler kurulu Unity'den dinamik cekilir.", () => Enum.GetNames(typeof(AndroidSdkVersions)), v =>
                {
                    PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)Enum.Parse(typeof(AndroidSdkVersions), v);
                    return null;
                }, () => PlayerSettings.Android.minSdkVersion.ToString()),
                Def1("player", "targetAPI", "Android hedef SDK versiyonu. Degerler kurulu Unity'den dinamik cekilir.", () => Enum.GetNames(typeof(AndroidSdkVersions)), v =>
                {
                    PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)Enum.Parse(typeof(AndroidSdkVersions), v);
                    return null;
                }, () => PlayerSettings.Android.targetSdkVersion.ToString()),

                Def1("quality", "qualityLevel", "Aktif Quality Level index'i (QualitySettings.SetQualityLevel).", null, v =>
                {
                    QualitySettings.SetQualityLevel(int.Parse(v), true);
                    return null;
                }, () => QualitySettings.GetQualityLevel().ToString()),
                Def1("quality", "vsync", "VSync Count (QualitySettings.vSyncCount).", null, v =>
                {
                    QualitySettings.vSyncCount = int.Parse(v);
                    return null;
                }, () => QualitySettings.vSyncCount.ToString(), defaultValue: "1"),

                Def1("time", "fixTime", "Fixed Timestep (Time.fixedDeltaTime).", null, v => { Time.fixedDeltaTime = float.Parse(v); return null; }, () => Time.fixedDeltaTime.ToString(), defaultValue: "0.02"),
                Def1("time", "maxAllowTime", "Maximum Allowed Timestep (Time.maximumDeltaTime).", null, v => { Time.maximumDeltaTime = float.Parse(v); return null; }, () => Time.maximumDeltaTime.ToString(), defaultValue: "0.3333333"),
                Def1("time", "timeScale", "Time.timeScale - bu bir proje ayari degil, Unity'nin canli runtime degeri. Sadece Play Mode'dayken kalici.", null, v =>
                {
                    Time.timeScale = float.Parse(v);
                    return null;
                }, () => Time.timeScale.ToString(), defaultValue: "1"),
                Def1("time", "maxPartTime", "Maximum Particle Timestep (Time.maximumParticleDeltaTime).", null, v => { Time.maximumParticleDeltaTime = float.Parse(v); return null; }, () => Time.maximumParticleDeltaTime.ToString(), defaultValue: "0.03"),

                Def1("physics", "sleepTh", "Sleep Threshold (Physics.sleepThreshold).", null, v => { Physics.sleepThreshold = float.Parse(v); return null; }, () => Physics.sleepThreshold.ToString(), defaultValue: "0.005"),
                Def1("physics", "contactOffset", "Default Contact Offset (Physics.defaultContactOffset).", null, v => { Physics.defaultContactOffset = float.Parse(v); return null; }, () => Physics.defaultContactOffset.ToString(), defaultValue: "0.01"),
                Def1("physics", "solverIter", "Default Solver Iterations (Physics.defaultSolverIterations).", null, v => { Physics.defaultSolverIterations = int.Parse(v); return null; }, () => Physics.defaultSolverIterations.ToString(), defaultValue: "6"),
                Def1("physics", "solverVelIter", "Default Solver Velocity Iterations (Physics.defaultSolverVelocityIterations).", null, v => { Physics.defaultSolverVelocityIterations = int.Parse(v); return null; }, () => Physics.defaultSolverVelocityIterations.ToString(), defaultValue: "1"),
                Def1("physics", "simMode", "Simulation Mode (Physics.simulationMode).", () => new[] { "FixedUpdate", "Update", "Script" }, v =>
                {
                    Physics.simulationMode = (SimulationMode)Enum.Parse(typeof(SimulationMode), v);
                    return null;
                }, () => Physics.simulationMode.ToString(), defaultValue: "FixedUpdate"),

                Def1("editor", "device", "Unity Remote - hedef cihaz (EditorSettings.unityRemoteDevice).", () => new[] { "None", "AnyAndroidDevice" }, v =>
                {
                    EditorSettings.unityRemoteDevice = v == "AnyAndroidDevice" ? "Any Android Device" : "None";
                    return null;
                }, () => EditorSettings.unityRemoteDevice == "Any Android Device" ? "AnyAndroidDevice" : "None", defaultValue: "None"),
                Def1("editor", "compression", "Unity Remote - goruntu sikistirma formati (EditorSettings.unityRemoteCompression).", () => new[] { "JPEG", "PNG" }, v =>
                {
                    EditorSettings.unityRemoteCompression = v;
                    return null;
                }, () => EditorSettings.unityRemoteCompression, defaultValue: "JPEG"),
                Def1("editor", "behavMode", "Default Behavior Mode, 3D/2D (EditorSettings.defaultBehaviorMode).", () => new[] { "3D", "2D" }, v =>
                {
                    EditorSettings.defaultBehaviorMode = v == "2D" ? EditorBehaviorMode.Mode2D : EditorBehaviorMode.Mode3D;
                    return null;
                }, () => EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D ? "2D" : "3D", defaultValue: "3D"),
                Def1("editor", "spriteMode", "Sprite Packer Mode (EditorSettings.spritePackerMode).", () => Enum.GetNames(typeof(SpritePackerMode)), v =>
                {
                    EditorSettings.spritePackerMode = (SpritePackerMode)Enum.Parse(typeof(SpritePackerMode), v);
                    return null;
                }, () => EditorSettings.spritePackerMode.ToString(), defaultValue: "Disabled"),
                Def1("editor", "playeMode", "Enter Play Mode Options: domain/scene reload kombinasyonu.", () => new[] { "ReloadDomainScene", "ReloadSceneOnly", "ReloadDomainOnly", "DoNotReload" }, v =>
                {
                    ApplyPlayMode(v);
                    return null;
                }, CurrentPlayMode, defaultValue: "ReloadDomainScene"),

                Def1("terminal", "fontSize", "Terminal yazi boyutu (px).", null, v =>
                {
                    if (!int.TryParse(v, out var size) || size < 6 || size > 72)
                        return "gecersiz boyut, 6-72 arasi bir sayi gir.";
                    TerminalTheme.FontSize = size;
                    TerminalTheme.RebuildAllWindows();
                    return null;
                }, () => TerminalTheme.FontSize.ToString(), defaultValue: "13"),
                Def1("terminal", "fontColor", "Terminal yazi rengi (hex #RRGGBB veya renk ismi).", () => _commonColorNames, v =>
                {
                    if (!ColorUtility.TryParseHtmlString(v, out var color))
                        return $"'{v}' gecerli bir renk degil. Ornek: #FFFFFF veya white.";
                    TerminalTheme.FontColor = color;
                    TerminalTheme.RebuildAllWindows();
                    return null;
                }, () => "#" + ColorUtility.ToHtmlStringRGB(TerminalTheme.FontColor), defaultValue: "#EBEBEB"),
                Def1("terminal", "bgColor", "Terminal arkaplan rengi (hex #RRGGBB veya renk ismi).", () => _commonColorNames, v =>
                {
                    if (!ColorUtility.TryParseHtmlString(v, out var color))
                        return $"'{v}' gecerli bir renk degil. Ornek: #000000 veya black.";
                    TerminalTheme.BackgroundColor = color;
                    TerminalTheme.RebuildAllWindows();
                    return null;
                }, () => "#" + ColorUtility.ToHtmlStringRGB(TerminalTheme.BackgroundColor), defaultValue: "#000000"),
            };
        }

        static bool Bool(string s) => string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);
        static string BoolStr(bool b) => b ? "true" : "false";

        static void ApplyPlayMode(string mode)
        {
            switch (mode)
            {
                case "ReloadDomainScene":
                    EditorSettings.enterPlayModeOptionsEnabled = false;
                    break;
                case "ReloadSceneOnly":
                    EditorSettings.enterPlayModeOptionsEnabled = true;
                    EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
                    break;
                case "ReloadDomainOnly":
                    EditorSettings.enterPlayModeOptionsEnabled = true;
                    EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableSceneReload;
                    break;
                case "DoNotReload":
                    EditorSettings.enterPlayModeOptionsEnabled = true;
                    EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
                    break;
            }
        }

        static string CurrentPlayMode()
        {
            if (!EditorSettings.enterPlayModeOptionsEnabled)
                return "ReloadDomainScene";

            var opts = EditorSettings.enterPlayModeOptions;
            if (opts == (EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload))
                return "DoNotReload";
            if (opts == EnterPlayModeOptions.DisableDomainReload)
                return "ReloadSceneOnly";
            if (opts == EnterPlayModeOptions.DisableSceneReload)
                return "ReloadDomainOnly";
            return "ReloadDomainScene";
        }

        static SettingDef Def1(string category, string key, string description, Func<IEnumerable<string>> values,
            Func<string, string> apply, Func<string> getter, string defaultValue = null)
        {
            IEnumerable<string> ValuesFor(int idx, string[] before)
            {
                if (idx != 0)
                    return null;

                var list = values?.Invoke()?.ToList() ?? new List<string>();
                if (defaultValue != null)
                    list.Add("-d");
                return list;
            }

            return new SettingDef
            {
                Category = category,
                Key = key,
                Description = description,
                DefaultValue = defaultValue,
                ValueSuggestions = ValuesFor,
                Apply = vals => vals.Length > 0 ? apply(vals[0]) : $"usage: set {category} {key} <value>",
                Getter = getter,
            };
        }

        public string Name => "set";

        public IEnumerable<string> GetSuggestions(int argIndex, string[] argsBeforeCurrent)
        {
            if (argIndex == 0)
                return _defs.Select(d => d.Category).Distinct();

            if (argIndex == 1 && argsBeforeCurrent.Length >= 1)
                return _defs.Where(d => d.Category == argsBeforeCurrent[0]).Select(d => d.Key);

            if (argIndex >= 2 && argsBeforeCurrent.Length >= 2)
            {
                var def = _defs.FirstOrDefault(d => d.Category == argsBeforeCurrent[0] && d.Key == argsBeforeCurrent[1]);
                if (def == null)
                    return null;

                var valueIndex = argIndex - 2;
                var valuesBefore = argsBeforeCurrent.Length > 2 ? argsBeforeCurrent.Skip(2).ToArray() : Array.Empty<string>();
                return def.ValueSuggestions?.Invoke(valueIndex, valuesBefore);
            }

            return null;
        }

        public string Execute(string[] args)
        {
            if (args.Length < 2)
                return "usage: set <category> <key> <value>";

            var def = _defs.FirstOrDefault(d => d.Category == args[0] && d.Key == args[1]);
            if (def == null)
                return $"'{args[0]} {args[1]}' is not a recognized setting.";

            if (args.Length == 3 && string.Equals(args[2], "-d", StringComparison.OrdinalIgnoreCase))
            {
                if (def.DefaultValue == null)
                    return $"'{args[0]} {args[1]}' icin bilinen bir varsayilan deger yok.";
                return def.Apply(new[] { def.DefaultValue });
            }

            if (args.Length < 3)
                return "usage: set <category> <key> <value>";

            var values = args.Skip(2).ToArray();
            return def.Apply(values);
        }

        public string GetHelp(string[] args)
        {
            if (args.Length == 0)
            {
                var categories = _defs.Select(d => d.Category).Distinct();
                return "set <kategori> <key> <deger> - bir Unity/Editor ayarini degistirir.\n" +
                       "Kategoriler: " + string.Join(", ", categories) + "\n" +
                       "Detay icin: set <kategori> -help  veya  set <kategori> <key> -help";
            }

            if (args.Length == 1)
            {
                var keys = _defs.Where(d => d.Category == args[0]).ToList();
                if (keys.Count == 0)
                    return $"'{args[0]}' gecerli bir kategori degil.";

                var lines = keys.Select(d => $"  {d.Key} - {d.Description}");
                return $"set {args[0]} <key> <deger>\n" + string.Join("\n", lines) +
                       $"\nDetay icin: set {args[0]} <key> -help";
            }

            var def = _defs.FirstOrDefault(d => d.Category == args[0] && d.Key == args[1]);
            if (def == null)
                return $"'{args[0]} {args[1]}' gecerli bir ayar degil.";

            var values = def.ValueSuggestions?.Invoke(0, Array.Empty<string>())?.Where(v => v != "-d").ToList();
            var valueHint = values != null && values.Count > 0 ? string.Join(", ", values) : "serbest metin/sayi";
            var defaultHint = def.DefaultValue != null
                ? $"\nVarsayilan: {def.DefaultValue} (set {args[0]} {args[1]} -d ile donebilirsin)"
                : "";

            return $"set {args[0]} {args[1]} <deger>\n{def.Description}\nGecerli degerler: {valueHint}{defaultHint}";
        }

        internal IEnumerable<string> Categories => _defs.Select(d => d.Category).Distinct();

        internal IEnumerable<(string Key, string Description)> KeysFor(string category) =>
            _defs.Where(d => d.Category == category).Select(d => (d.Key, d.Description));

        internal (bool Found, string Description, string Value) ReadValue(string category, string key)
        {
            var def = _defs.FirstOrDefault(d => d.Category == category && d.Key == key);
            if (def == null)
                return (false, null, null);
            return (true, def.Description, def.Getter != null ? def.Getter() : "(okuma tanimli degil)");
        }
    }
}

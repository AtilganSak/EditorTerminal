using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorTerminal
{
    public class EditorTerminalWindow : EditorWindow
    {
        private static readonly Color _tabStripBg = new Color(0.13f, 0.13f, 0.13f);
        private static readonly Color _tabBg = new Color(0.18f, 0.18f, 0.18f);
        private static readonly Color _tabActiveBg = Color.black;
        private static readonly Color _tabTextColor = new Color(0.8f, 0.8f, 0.8f);
        private static readonly Color _tabActiveTextColor = new Color(0.95f, 0.95f, 0.95f);
        private static readonly Color _tabBorderColor = new Color(0.05f, 0.05f, 0.05f);

        private const int _tabStripHeight = 26;

        [SerializeField] private List<TerminalSessionData> _history = new List<TerminalSessionData>();

        private static Font _monoFont;
        private VisualElement _tabStrip;
        private VisualElement _contentArea;
        private readonly List<TerminalSessionView> _sessions = new List<TerminalSessionView>();
        private TerminalSessionView _active;

        [MenuItem("Tools/Editor Terminal")]
        public static void Open()
        {
            var win = GetWindow<EditorTerminalWindow>(utility: true, title: "Terminal", focus: true);
            win.minSize = new Vector2(420, 240);
            win.Show();
        }

        void OnEnable()
        {
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void OnDisable()
        {
            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode) => Rebuild();

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
                Rebuild();
        }

        void OnCompilationStarted(object obj)
        {
            foreach (var session in _sessions)
                session.SetInputEnabled(false);
        }

        void OnCompilationFinished(object obj)
        {
            foreach (var session in _sessions)
                session.SetInputEnabled(true);
        }

        public static void SetAllBusy(string message)
        {
            foreach (var win in Resources.FindObjectsOfTypeAll<EditorTerminalWindow>())
                win._active?.BeginBusy(message);
        }

        public static void ClearAllBusy(string resultMessage)
        {
            foreach (var win in Resources.FindObjectsOfTypeAll<EditorTerminalWindow>())
                win._active?.EndBusy(resultMessage);
        }

        public void Rebuild() => CreateGUI();

        void CreateGUI()
        {
            if (_monoFont == null)
            {
                _monoFont = Font.CreateDynamicFontFromOSFont(
                    new[] { "Consolas", "Cascadia Mono", "Menlo", "Courier New" }, 13);

                _monoFont.hideFlags = HideFlags.DontSave;
            }

            CommandRegistry.Warmup();

            var root = rootVisualElement;
            root.Clear();
            root.style.flexGrow = 1;
            root.style.backgroundColor = Color.black;

            _sessions.Clear();

            _tabStrip = new VisualElement();
            _tabStrip.style.flexDirection = FlexDirection.Row;
            _tabStrip.style.alignItems = Align.Stretch;
            _tabStrip.style.backgroundColor = _tabStripBg;
            _tabStrip.style.height = _tabStripHeight;
            _tabStrip.style.borderBottomWidth = 1;
            _tabStrip.style.borderBottomColor = _tabBorderColor;
            root.Add(_tabStrip);

            _contentArea = new VisualElement();
            _contentArea.style.flexGrow = 1;
            root.Add(_contentArea);

            if (_history.Count == 0)
                AddSession();
            else
                RestoreSessions();
        }

        void RestoreSessions()
        {
            foreach (var data in _history)
            {
                var session = new TerminalSessionView(data, _monoFont);
                _sessions.Add(session);
                _contentArea.Add(session.Root);
            }

            SetActive(_sessions[0]);
        }

        TerminalSessionView AddSession()
        {
            var data = new TerminalSessionData { Title = $"Terminal {NextSessionNumber()}" };
            _history.Add(data);

            var session = new TerminalSessionView(data, _monoFont);
            _sessions.Add(session);
            _contentArea.Add(session.Root);

            SetActive(session);
            return session;
        }

        int NextSessionNumber()
        {
            var max = 0;
            foreach (var data in _history)
            {
                if (data.Title.StartsWith("Terminal ") && int.TryParse(data.Title.Substring("Terminal ".Length), out var n))
                    max = Mathf.Max(max, n);
            }
            return max + 1;
        }

        void CloseSession(TerminalSessionView session)
        {
            if (_sessions.Count <= 1)
                return;

            var index = _sessions.IndexOf(session);
            _sessions.RemoveAt(index);
            _contentArea.Remove(session.Root);
            _history.RemoveAll(d => d.Title == session.Title);

            if (_active == session)
            {
                var next = _sessions[Mathf.Max(0, index - 1)];
                SetActive(next);
            }
            else
            {
                RebuildTabStrip();
            }
        }

        void SetActive(TerminalSessionView session)
        {
            _active = session;
            foreach (var s in _sessions)
                s.Root.style.display = s == session ? DisplayStyle.Flex : DisplayStyle.None;

            RebuildTabStrip();
            session.Root.schedule.Execute(() => session.Focus());
        }

        void RebuildTabStrip()
        {
            _tabStrip.Clear();

            foreach (var session in _sessions)
                _tabStrip.Add(BuildTab(session));

            _tabStrip.Add(BuildAddButton());
        }

        VisualElement BuildTab(TerminalSessionView session)
        {
            var isActive = session == _active;

            var tab = new VisualElement();
            tab.style.flexDirection = FlexDirection.Row;
            tab.style.alignItems = Align.Center;
            tab.style.paddingLeft = 12;
            tab.style.paddingRight = 8;
            tab.style.backgroundColor = isActive ? _tabActiveBg : _tabBg;
            tab.style.borderRightWidth = 1;
            tab.style.borderRightColor = _tabBorderColor;

            var label = new Label(session.Title);
            label.style.unityFontDefinition = FontDefinition.FromFont(_monoFont);
            label.style.fontSize = 12;
            label.style.color = isActive ? _tabActiveTextColor : _tabTextColor;
            tab.Add(label);

            tab.RegisterCallback<MouseDownEvent>(_ => SetActive(session));

            if (_sessions.Count > 1)
            {
                var close = new Label("×");
                close.style.unityFontDefinition = FontDefinition.FromFont(_monoFont);
                close.style.fontSize = 13;
                close.style.color = _tabTextColor;
                close.style.marginLeft = 8;
                close.style.paddingLeft = 4;
                close.style.paddingRight = 4;
                close.RegisterCallback<MouseDownEvent>(evt =>
                {
                    evt.StopPropagation();
                    CloseSession(session);
                });
                tab.Add(close);
            }

            return tab;
        }

        VisualElement BuildAddButton()
        {
            var button = new Label("+");
            button.style.unityFontDefinition = FontDefinition.FromFont(_monoFont);
            button.style.fontSize = 14;
            button.style.color = _tabTextColor;
            button.style.paddingLeft = 12;
            button.style.paddingRight = 12;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            button.RegisterCallback<MouseDownEvent>(_ => AddSession());
            return button;
        }
    }
}

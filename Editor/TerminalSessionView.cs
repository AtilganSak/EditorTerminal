using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorTerminal
{
    public class TerminalSessionView
    {
        private const int _maxSuggestions = 8;
        private const int _maxHistory = 10;
        private const int _promptSpacerLines = 5;

        private static readonly Color _suggestionColor = new Color(0.6f, 0.6f, 0.6f);
        private static readonly Color _suggestionSelectedBg = new Color(0.25f, 0.45f, 0.85f);
        private static readonly Color _suggestionSelectedText = Color.white;

        public VisualElement Root { get; }
        public string Title => _data.Title;

        private readonly TerminalSessionData _data;
        private readonly ScrollView _output;
        private readonly Font _monoFont;
        private readonly List<string> _commandNames;
        private readonly List<string> _matches = new List<string>();
        private TextField _activeInput;
        private int _selectedSuggestion;
        private VisualElement _promptSpacer;

        public TerminalSessionView(TerminalSessionData data, Font monoFont)
        {
            _data = data;
            _monoFont = monoFont;
            _commandNames = new List<string>(CommandRegistry.GetCommandNames());

            Root = new VisualElement();
            Root.style.flexGrow = 1;
            Root.style.backgroundColor = TerminalTheme.BackgroundColor;

            _output = new ScrollView(ScrollViewMode.Vertical);
            _output.style.flexGrow = 1;
            _output.style.paddingLeft = 6;
            _output.style.paddingRight = 6;
            _output.style.paddingTop = 4;
            Root.Add(_output);

            ReplayHistory();
            AppendPromptRow();
        }

        public void Focus()
        {
            _activeInput?.Focus();
        }

        public void SetInputEnabled(bool enabled)
        {
            _activeInput?.SetEnabled(enabled);
        }

        private static readonly string[] _spinnerFrames = { "|", "/", "-", "\\" };

        private bool _busy;
        private Label _busyLabel;
        private IVisualElementScheduledItem _spinnerSchedule;

        public void BeginBusy(string message)
        {
            if (_busy)
                return;

            _busy = true;
            _activeInput = null;

            _busyLabel = new Label();
            _busyLabel.style.unityFontDefinition = FontDefinition.FromFont(_monoFont);
            _busyLabel.style.fontSize = TerminalTheme.FontSize;
            _busyLabel.style.color = TerminalTheme.FontColor;
            _output.Add(_busyLabel);

            var frame = 0;
            _spinnerSchedule = _busyLabel.schedule.Execute(() =>
            {
                _busyLabel.text = $"{_spinnerFrames[frame % _spinnerFrames.Length]} {message}";
                frame++;
            }).Every(120);

            ScrollToBottom();
        }

        public void EndBusy(string resultMessage)
        {
            if (!_busy)
                return;

            _busy = false;
            _spinnerSchedule?.Pause();
            _spinnerSchedule = null;

            if (_busyLabel != null)
            {
                _output.Remove(_busyLabel);
                _busyLabel = null;
            }

            if (!string.IsNullOrEmpty(resultMessage))
            {
                var colored = Colorize(resultMessage);
                _output.Add(NewTextLabel(colored));
                _data.Lines.Add(new TerminalLine { Text = colored, IsCommand = false });
            }

            AppendPromptRow();
        }

        private static string CurrentPromptText() => WorkingDirectory.Current + ">";

        void ReplayHistory()
        {
            foreach (var line in _data.Lines)
            {
                if (line.IsCommand)
                {
                    var row = NewCommandRow();
                    row.Add(NewPromptLabel(line.Prompt ?? CurrentPromptText()));
                    row.Add(NewTextLabel(line.Text));
                    _output.Add(row);
                }
                else
                {
                    _output.Add(NewTextLabel(line.Text));
                }
            }
        }

        private static VisualElement NewCommandRow()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginTop = 10;
            return row;
        }

        Label NewPromptLabel(string promptText)
        {
            var label = new Label(promptText);
            label.style.unityFontDefinition = FontDefinition.FromFont(_monoFont);
            label.style.fontSize = TerminalTheme.FontSize;
            label.style.color = TerminalTheme.FontColor;
            label.style.marginRight = 0;
            label.style.paddingRight = 0;
            return label;
        }

        Label NewTextLabel(string text)
        {
            var label = new Label(text);
            label.style.unityFontDefinition = FontDefinition.FromFont(_monoFont);
            label.style.fontSize = TerminalTheme.FontSize;
            label.style.color = TerminalTheme.FontColor;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.enableRichText = true;
            return label;
        }

        private const string _errorColorHex = "#E06C75";
        private const string _warnColorHex = "#E5C07B";

        private static string Colorize(string response)
        {
            var lines = response.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (StartsWithAny(line, "error:", "[error]"))
                    lines[i] = $"<color={_errorColorHex}>{line}</color>";
                else if (StartsWithAny(line, "usage:", "[warn]"))
                    lines[i] = $"<color={_warnColorHex}>{line}</color>";
            }
            return string.Join("\n", lines);
        }

        private static bool StartsWithAny(string line, params string[] prefixes)
        {
            foreach (var prefix in prefixes)
                if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        void AppendPromptRow()
        {
            var promptText = CurrentPromptText();

            var row = NewCommandRow();
            row.style.position = Position.Relative;
            row.Add(NewPromptLabel(promptText));

            var input = new TextField();
            input.style.flexGrow = 1;
            input.style.unityFontDefinition = FontDefinition.FromFont(_monoFont);
            input.style.fontSize = TerminalTheme.FontSize;
            input.style.color = TerminalTheme.FontColor;
            input.style.marginTop = 0;
            input.style.marginBottom = 0;
            input.style.marginLeft = 0;
            input.style.paddingLeft = 0;

            var inputElement = input.Q(className: "unity-base-text-field__input");
            if (inputElement != null)
            {
                inputElement.style.color = TerminalTheme.FontColor;
                inputElement.style.unityFontDefinition = FontDefinition.FromFont(_monoFont);
                inputElement.style.fontSize = TerminalTheme.FontSize;
                inputElement.style.backgroundColor = new Color(0, 0, 0, 0);
                inputElement.style.borderTopWidth = 0;
                inputElement.style.borderBottomWidth = 0;
                inputElement.style.borderLeftWidth = 0;
                inputElement.style.borderRightWidth = 0;
                inputElement.style.paddingLeft = 0;
                inputElement.style.paddingTop = 0;
                inputElement.style.paddingBottom = 0;
                inputElement.style.marginLeft = 0;
            }
            row.Add(input);

            input.cursorIndex = 0;
            input.selectIndex = 0;

            var suggestions = new VisualElement();
            suggestions.style.position = Position.Absolute;
            suggestions.style.top = Length.Percent(100);
            suggestions.style.flexDirection = FlexDirection.Column;
            suggestions.style.marginTop = 2;
            suggestions.style.backgroundColor = TerminalTheme.BackgroundColor;
            suggestions.style.display = DisplayStyle.None;
            row.Add(suggestions);

            var historyIndex = new[] { _data.History.Count };

            input.RegisterCallback<KeyDownEvent>(evt => OnInputKeyDown(evt, row, input, suggestions, promptText, historyIndex), TrickleDown.TrickleDown);
            input.RegisterValueChangedCallback(evt => UpdateSuggestions(suggestions, evt.newValue, promptText));

            _output.Add(row);

            _promptSpacer = new VisualElement();
            _promptSpacer.style.height = _promptSpacerLines * (TerminalTheme.FontSize + 6);
            _output.Add(_promptSpacer);

            _activeInput = input;

            input.SetEnabled(!EditorApplication.isCompiling);

            FocusWhenReady(input);
            ScrollToBottom();
        }

        private static void FocusWhenReady(VisualElement element)
        {
            void OnGeometryChanged(GeometryChangedEvent evt)
            {
                element.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                element.Focus();
            }
            element.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void UpdateSuggestions(VisualElement suggestions, string text, string promptText)
        {
            _matches.Clear();

            var wordStart = 0;

            if (!string.IsNullOrEmpty(text))
            {
                var tokens = CommandParser.Tokenize(text);
                var endsWithSpace = char.IsWhiteSpace(text[text.Length - 1]);
                var currentIndex = endsWithSpace ? tokens.Length : tokens.Length - 1;
                var currentPartial = endsWithSpace ? string.Empty : tokens[tokens.Length - 1];
                wordStart = endsWithSpace ? text.Length : text.Length - currentPartial.Length;

                IEnumerable<string> candidates = null;

                if (currentIndex == 0)
                {
                    candidates = _commandNames;
                }
                else if (currentIndex > 0 && CommandRegistry.TryGetCommand(tokens[0], out var command)
                    && command is ICommandArgSuggestions withArgs)
                {
                    var argsBefore = new string[currentIndex - 1];
                    for (var i = 0; i < argsBefore.Length; i++)
                        argsBefore[i] = tokens[i + 1];

                    candidates = withArgs.GetSuggestions(currentIndex - 1, argsBefore);
                }

                if (candidates != null)
                {
                    foreach (var candidate in candidates)
                    {
                        if (_matches.Count >= _maxSuggestions)
                            break;
                        if (candidate.StartsWith(currentPartial, StringComparison.OrdinalIgnoreCase))
                            _matches.Add(candidate);
                    }
                }

                if (_matches.Count == 1 && string.Equals(_matches[0], currentPartial, StringComparison.OrdinalIgnoreCase))
                    _matches.Clear();
            }

            suggestions.style.left = MeasureTextWidth(promptText + text?.Substring(0, wordStart));

            _selectedSuggestion = 0;
            RenderSuggestions(suggestions);
        }

        private GUIStyle _measureStyle;

        float MeasureTextWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0f;

            if (_measureStyle == null)
                _measureStyle = new GUIStyle { font = _monoFont };
            _measureStyle.fontSize = TerminalTheme.FontSize;

            return _measureStyle.CalcSize(new GUIContent(text)).x;
        }

        void RenderSuggestions(VisualElement suggestions)
        {
            suggestions.Clear();

            if (_matches.Count == 0)
            {
                suggestions.style.display = DisplayStyle.None;
                return;
            }

            for (var i = 0; i < _matches.Count; i++)
            {
                var isSelected = i == _selectedSuggestion;

                var item = new Label(_matches[i]);
                item.style.unityFontDefinition = FontDefinition.FromFont(_monoFont);
                item.style.fontSize = TerminalTheme.FontSize;
                item.style.paddingLeft = 6;
                item.style.paddingRight = 6;
                item.style.paddingTop = 1;
                item.style.paddingBottom = 1;
                item.style.color = isSelected ? _suggestionSelectedText : _suggestionColor;
                item.style.backgroundColor = isSelected ? _suggestionSelectedBg : new Color(0, 0, 0, 0);
                suggestions.Add(item);
            }

            suggestions.style.display = DisplayStyle.Flex;
        }

        void OnInputKeyDown(KeyDownEvent evt, VisualElement row, TextField input, VisualElement suggestions, string promptText, int[] historyIndex)
        {
            var hasSuggestions = _matches.Count > 0 && suggestions.style.display == DisplayStyle.Flex;

            if (hasSuggestions && evt.keyCode == KeyCode.DownArrow)
            {
                evt.StopPropagation();
                _selectedSuggestion = Mathf.Min(_selectedSuggestion + 1, _matches.Count - 1);
                RenderSuggestions(suggestions);
                return;
            }

            if (hasSuggestions && evt.keyCode == KeyCode.UpArrow)
            {
                evt.StopPropagation();
                _selectedSuggestion = Mathf.Max(_selectedSuggestion - 1, 0);
                RenderSuggestions(suggestions);
                return;
            }

            if (!hasSuggestions && evt.keyCode == KeyCode.UpArrow && _data.History.Count > 0)
            {
                evt.StopPropagation();
                historyIndex[0] = Mathf.Max(historyIndex[0] - 1, 0);
                SetInputValue(input, _data.History[historyIndex[0]]);
                return;
            }

            if (!hasSuggestions && evt.keyCode == KeyCode.DownArrow && _data.History.Count > 0)
            {
                evt.StopPropagation();
                historyIndex[0] = Mathf.Min(historyIndex[0] + 1, _data.History.Count);
                SetInputValue(input, historyIndex[0] < _data.History.Count ? _data.History[historyIndex[0]] : string.Empty);
                return;
            }

            if (hasSuggestions && evt.keyCode == KeyCode.Tab)
            {
                evt.StopPropagation();
                input.focusController?.IgnoreEvent(evt);

                var text = input.value;
                var endsWithSpace = text.Length > 0 && char.IsWhiteSpace(text[text.Length - 1]);
                var lastSpace = text.LastIndexOf(' ');
                var prefix = endsWithSpace ? text : lastSpace < 0 ? string.Empty : text.Substring(0, lastSpace + 1);

                var newValue = prefix + _matches[_selectedSuggestion] + " ";
                SetInputValue(input, newValue);

                UpdateSuggestions(suggestions, newValue, promptText);
                return;
            }

            var isEnter = evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter
                || evt.character == '\n' || evt.character == '\r';
            if (!isEnter)
                return;

            evt.StopPropagation();

            if (!row.Contains(input))
                return;

            var command = input.value;
            if (string.IsNullOrWhiteSpace(command))
                return;

            PushHistory(command);

            if (string.Equals(command, "clear", StringComparison.OrdinalIgnoreCase))
            {
                _data.Lines.Clear();
                _output.Clear();
                AppendPromptRow();
                return;
            }

            row.Remove(suggestions);
            _output.Remove(_promptSpacer);

            row.Remove(input);
            row.Add(NewTextLabel(command));
            _data.Lines.Add(new TerminalLine { Text = command, IsCommand = true, Prompt = promptText });

            var response = Execute(command);
            if (!string.IsNullOrEmpty(response))
            {
                var colored = Colorize(response);
                _output.Add(NewTextLabel(colored));
                _data.Lines.Add(new TerminalLine { Text = colored, IsCommand = false });
            }

            if (!_busy)
                AppendPromptRow();
        }

        string Execute(string command)
        {
            return CommandRegistry.Execute(command);
        }

        void PushHistory(string command)
        {
            _data.History.Add(command);
            if (_data.History.Count > _maxHistory)
                _data.History.RemoveAt(0);
        }

        private static void SetInputValue(TextField input, string value)
        {
            input.SetValueWithoutNotify(value);
            input.cursorIndex = value.Length;
            input.selectIndex = value.Length;
        }

        void ScrollToBottom()
        {
            _output.contentContainer.RegisterCallback<GeometryChangedEvent>(OnContentGeometryChanged);
        }

        void OnContentGeometryChanged(GeometryChangedEvent evt)
        {
            _output.contentContainer.UnregisterCallback<GeometryChangedEvent>(OnContentGeometryChanged);
            _output.scrollOffset = new Vector2(0, float.MaxValue);
        }
    }
}

# Editor Terminal

A CMD-style command line inside a Unity `EditorWindow`. Drives common editor/project tasks
(creating scripts, opening windows, changing Player/Quality/Time/Physics settings, file
navigation, Package Manager, builds) without leaving the keyboard.

Built for Unity 6 (6000.4.0f1), URP. Source lives entirely under `Assets/EditorTerminal/`.

## Install

**Via Unity Package Manager (Git URL)**

`Window > Package Manager > + > Add package from git URL...`, paste:

```
https://github.com/AtilganSak/EditorTerminal.git
```

## Usage

Open via `Tools > Editor Terminal`. Each click opens a new independent window instance.

- **Tabs** - a window can hold multiple terminal tabs; `+` opens a new one, `×` on a tab closes it
  (the last remaining tab can't be closed).
- **Tab-completion** - start typing a command/argument and a suggestion list appears; `Tab` accepts
  the highlighted one, `↑`/`↓` move the selection while the list is open.
- **Command history** - when no suggestion list is open, `↑`/`↓` cycle through the last 10 commands
  submitted in that tab.

Type `-help` alone in the terminal for the full list with one-line descriptions, or
`<command> -help` for details on any single command.

### File / folder navigation

- `ls` - lists folders/files in the current directory
- `cd <path>` - changes directory (`cd ..` goes up one level, `cd -r` resets to `Assets`)
- `del <path>` - deletes a file/folder
- `copy <source> <target>` - copies a file/folder
- `move <source> <target>` - moves/renames a file/folder
- `select <path>` - selects + pings it in the Project window

### Creation (`create`)

- `create mono <name> [path]` - blank MonoBehaviour script
- `create editor <name> <target script> [path]` - custom Inspector (Editor) script
- `create editorWindow <name> [path]` - blank EditorWindow script
- `create so <name> [file name] [menu name] [path]` - ScriptableObject script
- `create class <name> [path]` - blank C# class
- `create struct <name> [path]` - blank C# struct
- `create material <name> [color] [path]` - new Material asset
- `create scene <name> [path]` - new scene
- `create folder <name> [path]` - new folder

### Project settings (`set` / `get`)

- `set <category> <key> <value>` - changes a Player/Quality/Time/Physics/Editor/terminal setting
- `set <category> <key> -d` - resets that setting to its known default (where one is defined)
- `get <category> <key>` - reads a setting's current value

### Project definitions (`add` / `remove`)

- `add tag <name>` - new Tag
- `add layer <name>` - new Layer
- `add sortingLayer <name>` - new Sorting Layer
- `add SDS <symbol>` - new Scripting Define Symbol
- `add scene <name or path>` - adds the scene to Build Settings
- `add package <id>` - adds a package via Package Manager
- `remove package <id>` - removes a package via Package Manager

### Editor windows (`open`)

- `open projectSettings` / `preferences` / `lighting` / `animation` / `animator` / `audioMixer` /
  `navigation` / `projectWindow` / `console` / `hierarchy` / `inspector`
- `open scene <name or path>` - opens a scene

### Build & Play Mode

- `build <output folder> [true|false]` - builds the player into a file named after Product Name
  (Windows/Mac/Linux get the right extension automatically); second arg triggers Build And Run
- `play` - enters Play Mode
- `pause` - pauses/resumes while in Play Mode
- `stop` - exits Play Mode

### Other

- `print <text>` - prints text to the terminal
- `clear` - clears this terminal tab's history
- `log [n]` - prints console messages captured since this terminal was opened
- `import <path>` - imports a `.unitypackage` file
- `quit` - closes the Editor (prompts to save first)

## Notes

- No CLI build or test suite - this is Editor-only tooling, verified interactively.
- File-system commands (`cd`/`del`/`copy`/`move`/`create`) can't escape the `Assets/` folder.
- Terminal history/scrollback and `cd` location both survive domain reload (script recompiles).
  Run `cd -r` to reset the location back to `Assets`.

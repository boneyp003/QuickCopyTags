# QuickCopyTags

A small desktop app for quickly copying predefined text snippets ("tags") to the clipboard — useful for job applications and other tasks that involve a lot of repeated copy-pasting (cover letter intros, "why this company" blurbs, skills summaries, etc.).

Built with .NET 8+ and [Avalonia UI](https://avaloniaui.net/), runs on Linux and Windows.

> **⚠️ Personal use only.** This app is intended for a single user on their own machine, not for handling sensitive data. Tags are stored **unencrypted** on disk, and copying a tag puts its plaintext content on the system clipboard, readable by any other app. **Do not store passwords, API keys, tokens, or other sensitive/confidential information in a tag.**

## How it works

- Launch it like any normal desktop app — a window opens with a grid of your tags.
- Click a tag to copy its text to the clipboard.
- Manage your tags (add, edit, delete, reorder via drag-and-drop or buttons) from the Settings window, opened from a button in the main window.
- Closing the window (the X button) exits the app completely — it does not run in the background.

## Project layout

```
QuickCopyTags/
  App.axaml(.cs)          App startup, main window creation
  Program.cs               Entry point
  Models/                  Tag, TagData
  Services/                TagStore (persistence)
  ViewModels/               SettingsViewModel
  Views/                    MainWindow, SettingsWindow
  Assets/                   App icon (icon.ico/icon.png, generated from QTC_icon.png)
package/
  build-deb.sh              Builds a self-contained .deb package for Ubuntu/Debian
```

Tags are stored as JSON in the OS user config directory (`~/.config/QuickCopyTags/tags.json` on Linux, `%AppData%\QuickCopyTags\tags.json` on Windows).

## Building and running from source

Requires the .NET SDK.

```bash
cd QuickCopyTags
dotnet run
```

## Building an installable package (Ubuntu/Debian)

```bash
./package/build-deb.sh          # builds version 1.0.0
./package/build-deb.sh 1.2.0    # or a specific version
```

This publishes a self-contained `linux-x64` build and packages it as a `.deb`, output to `dist/quickcopytags_<version>_amd64.deb`. Install with:

```bash
sudo apt install ./dist/quickcopytags_<version>_amd64.deb
```

This installs the app to `/opt/quickcopytags`, adds an app menu entry and icon, and creates a `quickcopytags` command on `PATH`. Uninstall with:

```bash
sudo apt remove quickcopytags
```

Your saved tags (`~/.config/QuickCopyTags/tags.json`) are untouched by install/uninstall.

## Publishing for Windows

```bash
dotnet publish QuickCopyTags -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## TODO / ideas

- [ ] Placeholder variables in tags (e.g. `{{company}}`, `{{role}}`) with an inline fill-in prompt before copying
- [ ] Search/filter box in the main window
- [x] Categories/folders for grouping tags
- [ ] Usage-based ordering (most-used tags float to the top)
- [ ] Import/export tags for backup or moving between machines

## License

[MIT](LICENSE)

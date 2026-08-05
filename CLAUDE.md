# QuickCopyTags

A small Avalonia (.NET 10) desktop app for copying predefined text snippets ("tags") to the
clipboard — built for one person's own job-application workflow. Runs on Linux and Windows.
**Personal use only** — tags are stored unencrypted; never suggest storing credentials/secrets
in a tag. See `README.md` for the full user-facing description.

## Architecture

- `App.axaml.cs` — composition root. Creates `MainWindow` on startup, owns the single
  `SettingsWindow` instance (opened on demand, reused if already open).
- `Views/MainWindow` — the only window shown at launch. Groups tags into collapsible
  per-category `Expander` sections (code-behind driven, not full MVVM — see `RefreshTags()`).
  "Uncategorized" is always the last section and isn't a real `Category` row, just tags whose
  `CategoryId` is null or doesn't match a known category.
- `Views/SettingsWindow` + `ViewModels/SettingsViewModel` — tag CRUD, category assignment,
  tag font size. Full MVVM here (CommunityToolkit.Mvvm `[ObservableProperty]`/`[RelayCommand]`).
- `Views/CategoriesWindow` + `ViewModels/CategoriesViewModel` — "Manage Categories" dialog,
  opened from Settings. Operates on the **same** `ObservableCollection<Category>`/`<Tag>`
  instances held by `SettingsViewModel` (passed by reference), so edits here show up live in
  the Settings tag editor's category dropdown without a reload. Deleting a category reassigns
  its tags to Uncategorized rather than deleting them.
- `Services/TagStore` — all persistence. Tags/Categories/font-size/etc. live in one JSON file;
  its *location* is itself redirectable (Settings → "Tag-set file"), tracked via a small
  pointer file (`location.json`) in the OS config dir. Don't assume the tags file is always at
  the default path — always go through `TagStore.FilePath`/`Load()`/`Save()`.
- No tray icon, no background residency, no global hotkey — closing the main window's X button
  fully quits the app (`ShutdownMode.OnMainWindowClose` in `App.axaml.cs`). This was deliberate;
  earlier versions had a tray+hotkey model that was removed by request.

## Conventions

- Styling is dark-theme, hardcoded hex colors (`#1e1e1e` etc.) — doesn't adapt to light OS
  theme. Known, accepted tradeoff, not a bug.
- `MainWindow`'s tag/category font size is applied via **inheritance**: `SectionsList.FontSize`
  is set once in code-behind and cascades down to `Expander` headers and tag `Button`s, rather
  than binding FontSize on every element. Category headers are `FontWeight="Bold"`, tags are
  explicitly `FontWeight="Normal"` (don't rely on inheritance for weight — it doesn't cascade
  the same way in practice here).
- Avalonia gotchas hit in this codebase, worth remembering before re-debugging them:
  - A `ScrollViewer` inside a `StackPanel` never scrolls (unbounded height). Needs a `Grid`
    row sized `*`.
  - `Expander`'s focus-visible outline only renders for keyboard-style focus — call
    `.Focus(NavigationMethod.Tab)`, not the parameterless overload.
  - `ListBox.ScrollIntoView`/`UpdateLayout` can throw `InvalidOperationException` ("Invalid
    Arrange rectangle") from a `VirtualizingStackPanel` layout bug under some list states.
    This is wrapped in a try/catch in `SettingsWindow.ScrollToAndFocus` since it's a cosmetic
    affordance, not worth crashing the app over.
- No automated tests exist. Verification is manual: `dotnet build`, launch the binary in the
  background, check the log for exceptions, confirm the process stays alive. When testing
  against the real `tags.json` (`~/.config/QuickCopyTags/tags.json`), back it up first — it
  contains the user's real personal data (name, contact info, resume bullet points).

## Commands

```bash
# Run from source
cd QuickCopyTags && dotnet run

# Build (from QuickCopyTags/QuickCopyTags/)
dotnet build -c Debug

# Package for Ubuntu/Debian → dist/quickcopytags_<version>_amd64.deb
./package/build-deb.sh [version]

# Package for Windows (needs Inno Setup 6) → dist/quickcopytags_<version>_win-x64_setup.exe
./package/build-windows.ps1 [-Version <version>]
```

## Git workflow

- `main` is protected by convention (not enforced) — work on a `feature/*` branch, push, open
  a PR with `gh pr create`. `gh` is authenticated as `boneyp003`.
- Delete feature branches (local + remote) after merge.

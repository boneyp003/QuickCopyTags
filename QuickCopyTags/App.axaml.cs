using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using QuickCopyTags.Services;
using QuickCopyTags.ViewModels;
using QuickCopyTags.Views;

namespace QuickCopyTags;

/// <summary>Composition root: creates the main window and owns the single Settings window instance.</summary>
public partial class App : Application
{
    private TagStore? _tagStore;
    private MainWindow? _mainWindow;
    private SettingsWindow? _settings;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Closing the main window quits the app even if the Settings window is still open.
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;

            _tagStore = new TagStore();
            _mainWindow = new MainWindow(_tagStore, OpenSettings);
            desktop.MainWindow = _mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OpenSettings()
    {
        if (_tagStore is null)
        {
            return;
        }

        if (_settings is null || !_settings.IsVisible)
        {
            _settings = new SettingsWindow(new SettingsViewModel(_tagStore));
            _settings.Closed += (_, _) => _mainWindow?.RefreshTags();
            _settings.Show();
        }
        else
        {
            _settings.Activate();
        }
    }
}

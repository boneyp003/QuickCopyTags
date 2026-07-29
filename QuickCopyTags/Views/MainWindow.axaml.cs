using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using QuickCopyTags.Models;
using QuickCopyTags.Services;

namespace QuickCopyTags.Views;

public partial class MainWindow : Window
{
    private readonly TagStore _tagStore;
    private readonly ObservableCollection<Tag> _tags = new();
    private readonly Action _onSettingsRequested;

    public MainWindow(TagStore tagStore, Action onSettingsRequested)
    {
        _tagStore = tagStore;
        _onSettingsRequested = onSettingsRequested;
        InitializeComponent();
        TagsList.ItemsSource = _tags;
        RefreshTags();
    }

    public void RefreshTags()
    {
        _tags.Clear();
        foreach (var tag in _tagStore.Load())
        {
            _tags.Add(tag);
        }
    }

    private async void OnTagButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: Tag tag })
        {
            var clipboard = GetTopLevel(this)?.Clipboard;
            if (clipboard is not null)
            {
                await clipboard.SetTextAsync(tag.Content);
            }
        }
    }

    private void OnSettingsButtonClick(object? sender, RoutedEventArgs e) => _onSettingsRequested();
}

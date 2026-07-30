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
    private readonly ObservableCollection<CategorySection> _sections = new();
    private readonly Action _onSettingsRequested;

    public MainWindow(TagStore tagStore, Action onSettingsRequested)
    {
        _tagStore = tagStore;
        _onSettingsRequested = onSettingsRequested;
        InitializeComponent();
        SectionsList.ItemsSource = _sections;
        RefreshTags();
    }

    public void RefreshTags()
    {
        var data = _tagStore.Load();

        // FontSize inherits down to the Expander headers and tag buttons within, so this
        // single assignment keeps categories and tags at the same, user-adjustable size.
        SectionsList.FontSize = data.TagFontSize;

        _sections.Clear();
        foreach (var section in BuildSections(data))
        {
            _sections.Add(section);
        }
    }

    private static IEnumerable<CategorySection> BuildSections(TagData data)
    {
        var categoryIds = data.Categories.Select(c => c.Id).ToHashSet();

        foreach (var category in data.Categories)
        {
            var tagsInCategory = data.Tags.Where(t => t.CategoryId == category.Id);
            yield return new CategorySection(category.Name, tagsInCategory);
        }

        var uncategorizedTags = data.Tags.Where(t => t.CategoryId is null || !categoryIds.Contains(t.CategoryId));
        yield return new CategorySection("Uncategorized", uncategorizedTags);
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

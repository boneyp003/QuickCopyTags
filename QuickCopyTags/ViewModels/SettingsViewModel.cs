using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuickCopyTags.Models;
using QuickCopyTags.Services;

namespace QuickCopyTags.ViewModels;

/// <summary>Backs the tag editor: the Tags list, Category assignment, and the main window's tag font size.</summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly TagStore _tagStore;

    public ObservableCollection<Tag> Tags { get; } = new();

    public ObservableCollection<Category> Categories { get; } = new();

    /// <summary>Categories plus a synthetic leading "Uncategorized" (Id null) entry, for the tag editor's dropdown.</summary>
    public ObservableCollection<Category> CategoryOptions { get; } = new();

    public List<int> TagFontSizeOptions { get; } = new() { 9, 10, 11, 12, 14, 16, 18, 20 };

    [ObservableProperty]
    private int _tagFontSize = 11;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteTagCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveUpCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveDownCommand))]
    private Tag? _selectedTag;

    /// <summary>Raised after a tag's position changes, so the view can scroll/focus it.</summary>
    public event Action<Tag>? TagMoved;

    public SettingsViewModel(TagStore tagStore)
    {
        _tagStore = tagStore;
        Categories.CollectionChanged += (_, _) => RebuildCategoryOptions();
        PopulateFrom(_tagStore.Load());
    }

    /// <summary>Replaces the Tags/Categories/font size shown in the editor with the given data,
    /// clearing the current selection. Used for the initial load and after switching tag files.</summary>
    private void PopulateFrom(TagData data)
    {
        TagFontSize = data.TagFontSize;
        SelectedTag = null;

        Tags.Clear();
        foreach (var tag in data.Tags)
        {
            Tags.Add(tag);
        }

        Categories.Clear();
        foreach (var category in data.Categories)
        {
            Categories.Add(category);
        }

        RebuildCategoryOptions();
    }

    private void RebuildCategoryOptions()
    {
        CategoryOptions.Clear();
        CategoryOptions.Add(new Category { Id = null!, Name = "Uncategorized" });
        foreach (var category in Categories)
        {
            CategoryOptions.Add(category);
        }
    }

    [RelayCommand]
    private void AddTag()
    {
        var tag = new Tag { Label = "New Tag", Content = "" };
        Tags.Add(tag);
        SelectedTag = tag;
        Save();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void DeleteTag()
    {
        if (SelectedTag is null)
        {
            return;
        }

        Tags.Remove(SelectedTag);
        SelectedTag = null;
        Save();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void MoveUp()
    {
        if (SelectedTag is null)
        {
            return;
        }

        var index = Tags.IndexOf(SelectedTag);
        if (index > 0)
        {
            Tags.Move(index, index - 1);
            Save();
            TagMoved?.Invoke(SelectedTag);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void MoveDown()
    {
        if (SelectedTag is null)
        {
            return;
        }

        var index = Tags.IndexOf(SelectedTag);
        if (index >= 0 && index < Tags.Count - 1)
        {
            Tags.Move(index, index + 1);
            Save();
            TagMoved?.Invoke(SelectedTag);
        }
    }

    public void ReorderTag(string draggedTagId, Tag target)
    {
        var draggedIndex = Tags.ToList().FindIndex(t => t.Id == draggedTagId);
        var targetIndex = Tags.IndexOf(target);
        if (draggedIndex < 0 || targetIndex < 0 || draggedIndex == targetIndex)
        {
            return;
        }

        var draggedTag = Tags[draggedIndex];
        Tags.Move(draggedIndex, targetIndex);
        SelectedTag = draggedTag;
        Save();
        TagMoved?.Invoke(draggedTag);
    }

    private bool HasSelection() => SelectedTag is not null;

    public void Save() => _tagStore.Save(new TagData { Tags = Tags.ToList(), Categories = Categories.ToList(), TagFontSize = TagFontSize });

    /// <summary>Points the store at a different existing JSON file and reloads from it. Returns null on
    /// success, or an error message if the file couldn't be used.</summary>
    public string? ChangeTagFileLocation(string newPath)
    {
        var error = _tagStore.ChangeFilePath(newPath);
        if (error is not null)
        {
            return error;
        }

        PopulateFrom(_tagStore.Load());
        return null;
    }

    public CategoriesViewModel CreateCategoriesViewModel() => new(_tagStore, Categories, Tags, TagFontSize);
}

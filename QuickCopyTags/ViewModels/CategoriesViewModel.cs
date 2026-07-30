using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuickCopyTags.Models;
using QuickCopyTags.Services;

namespace QuickCopyTags.ViewModels;

/// <summary>
/// Manages the Categories list. Operates on the same Categories/Tags collection instances
/// held by SettingsViewModel, so changes made here (add/rename/delete/reorder) are reflected
/// immediately in the tag editor's category dropdown without needing to reload.
/// </summary>
public partial class CategoriesViewModel : ObservableObject
{
    private readonly TagStore _tagStore;
    private readonly ObservableCollection<Tag> _tags;
    private readonly int _tagFontSize;

    public ObservableCollection<Category> Categories { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCategoryCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveUpCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveDownCommand))]
    private Category? _selectedCategory;

    public CategoriesViewModel(TagStore tagStore, ObservableCollection<Category> categories, ObservableCollection<Tag> tags, int tagFontSize)
    {
        _tagStore = tagStore;
        Categories = categories;
        _tags = tags;
        _tagFontSize = tagFontSize;
    }

    [RelayCommand]
    private void AddCategory()
    {
        var category = new Category { Name = "New Category" };
        Categories.Add(category);
        SelectedCategory = category;
        Save();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void DeleteCategory()
    {
        if (SelectedCategory is null)
        {
            return;
        }

        foreach (var tag in _tags)
        {
            if (tag.CategoryId == SelectedCategory.Id)
            {
                tag.CategoryId = null;
            }
        }

        Categories.Remove(SelectedCategory);
        SelectedCategory = null;
        Save();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void MoveUp()
    {
        if (SelectedCategory is null)
        {
            return;
        }

        var index = Categories.IndexOf(SelectedCategory);
        if (index > 0)
        {
            Categories.Move(index, index - 1);
            Save();
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void MoveDown()
    {
        if (SelectedCategory is null)
        {
            return;
        }

        var index = Categories.IndexOf(SelectedCategory);
        if (index >= 0 && index < Categories.Count - 1)
        {
            Categories.Move(index, index + 1);
            Save();
        }
    }

    public void ReorderCategory(string draggedCategoryId, Category target)
    {
        var draggedIndex = Categories.ToList().FindIndex(c => c.Id == draggedCategoryId);
        var targetIndex = Categories.IndexOf(target);
        if (draggedIndex < 0 || targetIndex < 0 || draggedIndex == targetIndex)
        {
            return;
        }

        var draggedCategory = Categories[draggedIndex];
        Categories.Move(draggedIndex, targetIndex);
        SelectedCategory = draggedCategory;
        Save();
    }

    private bool HasSelection() => SelectedCategory is not null;

    public void Save() => _tagStore.Save(new TagData { Tags = _tags.ToList(), Categories = Categories.ToList(), TagFontSize = _tagFontSize });
}

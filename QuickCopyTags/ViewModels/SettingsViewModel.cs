using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuickCopyTags.Models;
using QuickCopyTags.Services;

namespace QuickCopyTags.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly TagStore _tagStore;

    public ObservableCollection<Tag> Tags { get; } = new();

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
        foreach (var tag in _tagStore.Load())
        {
            Tags.Add(tag);
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
            var tag = SelectedTag;
            Tags.Move(index, index - 1);
            Save();
            TagMoved?.Invoke(tag);
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
            var tag = SelectedTag;
            Tags.Move(index, index + 1);
            Save();
            TagMoved?.Invoke(tag);
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

    public void Save() => _tagStore.Save(Tags.ToList());
}

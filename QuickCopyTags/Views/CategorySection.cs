using System.Collections.ObjectModel;
using QuickCopyTags.Models;

namespace QuickCopyTags.Views;

/// <summary>A UI-only grouping of tags under a category name, for display in MainWindow.</summary>
public class CategorySection
{
    public CategorySection(string name, IEnumerable<Tag> tags)
    {
        Name = name;
        Tags = new ObservableCollection<Tag>(tags);
    }

    public string Name { get; }

    public ObservableCollection<Tag> Tags { get; }

    public bool IsEmpty => Tags.Count == 0;
}

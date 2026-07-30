using CommunityToolkit.Mvvm.ComponentModel;

namespace QuickCopyTags.Models;

public partial class Tag : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString("N");

    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private string _content = string.Empty;

    /// <summary>Null means the tag is Uncategorized.</summary>
    [ObservableProperty]
    private string? _categoryId;
}

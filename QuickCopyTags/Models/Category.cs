using CommunityToolkit.Mvvm.ComponentModel;

namespace QuickCopyTags.Models;

public partial class Category : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString("N");

    [ObservableProperty]
    private string _name = string.Empty;
}

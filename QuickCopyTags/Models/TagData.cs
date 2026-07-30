namespace QuickCopyTags.Models;

public class TagData
{
    public List<Tag> Tags { get; set; } = new();

    public List<Category> Categories { get; set; } = new();

    /// <summary>Font size used for tags and category headers in the main window.</summary>
    public int TagFontSize { get; set; } = 11;
}

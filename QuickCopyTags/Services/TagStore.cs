using System.Text.Json;
using QuickCopyTags.Models;

namespace QuickCopyTags.Services;

public class TagStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _filePath;

    public TagStore()
    {
        var configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "QuickCopyTags");
        Directory.CreateDirectory(configDir);
        _filePath = Path.Combine(configDir, "tags.json");
    }

    public List<Tag> Load()
    {
        if (!File.Exists(_filePath))
        {
            var seeded = SeedDefaults();
            Save(seeded);
            return seeded;
        }

        var json = File.ReadAllText(_filePath);
        var data = JsonSerializer.Deserialize<TagData>(json);
        return data?.Tags ?? new List<Tag>();
    }

    public void Save(List<Tag> tags)
    {
        var data = new TagData { Tags = tags };
        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private static List<Tag> SeedDefaults() =>
        new()
        {
            new Tag { Label = "Cover Letter Intro", Content = "Dear Hiring Manager,\n\nI'm excited to apply for this role..." },
            new Tag { Label = "Why This Company", Content = "I've long admired your company's work in..." },
            new Tag { Label = "Skills Summary", Content = "Experienced software engineer with a background in..." },
        };
}

using System.Text.Json;
using QuickCopyTags.Models;

namespace QuickCopyTags.Services;

/// <summary>Loads and saves tags, categories, and display settings from a single JSON file in the user's config directory.</summary>
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

    public TagData Load()
    {
        if (!File.Exists(_filePath))
        {
            var seeded = SeedDefaults();
            Save(seeded);
            return seeded;
        }

        var json = File.ReadAllText(_filePath);
        var data = JsonSerializer.Deserialize<TagData>(json);
        return data ?? new TagData();
    }

    public void Save(TagData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private static TagData SeedDefaults() =>
        new()
        {
            Tags = new List<Tag>
            {
                new Tag { Label = "Cover Letter Intro", Content = "Dear Hiring Manager,\n\nI'm excited to apply for this role..." },
                new Tag { Label = "Why This Company", Content = "I've long admired your company's work in..." },
                new Tag { Label = "Skills Summary", Content = "Experienced software engineer with a background in..." },
            },
        };
}

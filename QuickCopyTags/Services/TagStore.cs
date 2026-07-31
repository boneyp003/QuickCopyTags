using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using QuickCopyTags.Models;

namespace QuickCopyTags.Services;

/// <summary>Loads and saves tags, categories, and display settings from a single JSON file, whose location
/// is itself recorded in a small pointer file in the user's config directory, so it can be redirected to
/// another file (see <see cref="ChangeFilePath"/>).</summary>
public class TagStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _pointerPath;

    public string FilePath { get; private set; }

    public TagStore()
    {
        var configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "QuickCopyTags");
        Directory.CreateDirectory(configDir);
        _pointerPath = Path.Combine(configDir, "location.json");
        FilePath = ReadPointer() ?? Path.Combine(configDir, "tags.json");
    }

    public TagData Load()
    {
        if (!File.Exists(FilePath))
        {
            var seeded = SeedDefaults();
            Save(seeded);
            return seeded;
        }

        var json = File.ReadAllText(FilePath);
        var data = JsonSerializer.Deserialize<TagData>(json);
        return data ?? new TagData();
    }

    public void Save(TagData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    /// <summary>Switches to loading/saving tags from a different existing JSON file. Returns null on success,
    /// or an error message if the file doesn't exist or isn't valid tag data.</summary>
    public string? ChangeFilePath(string newPath)
    {
        if (!File.Exists(newPath))
        {
            return "File not found.";
        }

        if (!TryDeserialize<TagData>(newPath, out _))
        {
            return "That file isn't a valid QuickCopyTags JSON file.";
        }

        FilePath = newPath;
        File.WriteAllText(_pointerPath, JsonSerializer.Serialize(new TagsFilePointer { TagsFilePath = newPath }, JsonOptions));
        return null;
    }

    private string? ReadPointer()
    {
        if (!File.Exists(_pointerPath))
        {
            return null;
        }

        return TryDeserialize<TagsFilePointer>(_pointerPath, out var pointer)
            && pointer.TagsFilePath is { Length: > 0 } path
            && File.Exists(path)
                ? path
                : null;
    }

    /// <summary>Deserializes the file at <paramref name="path"/>, treating malformed JSON or a JSON `null`
    /// as failure rather than letting <see cref="JsonException"/> propagate to callers that only care
    /// whether it worked.</summary>
    private static bool TryDeserialize<T>(string path, [NotNullWhen(true)] out T? data) where T : class
    {
        try
        {
            data = JsonSerializer.Deserialize<T>(File.ReadAllText(path));
            return data is not null;
        }
        catch (JsonException)
        {
            data = null;
            return false;
        }
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

    private class TagsFilePointer
    {
        public string TagsFilePath { get; set; } = string.Empty;
    }
}

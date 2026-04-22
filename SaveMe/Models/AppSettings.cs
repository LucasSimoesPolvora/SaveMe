using System.Text.Json.Serialization;

namespace SaveMe.Models;

public class AppSettings
{
    [JsonPropertyName("saveMePaths")]
    public List<string> SaveMePaths { get; set; } = new();
}

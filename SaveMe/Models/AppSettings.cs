using System.Text.Json.Serialization;

namespace SaveMe.Models;

public class AppSettings
{
    [JsonPropertyName("saveMePath")]
    public string SaveMePath { get; set; } = string.Empty;
}

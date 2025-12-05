using System.Text.Json.Serialization;

namespace SpotifyOrganizer.Models.Profile
{
    public class ImageObject
    {
        [JsonPropertyName("url")] public string Url { get; set; }
        [JsonPropertyName("height")] public int? Height { get; set; }
        [JsonPropertyName("width")] public int? Width { get; set; }
    }
}

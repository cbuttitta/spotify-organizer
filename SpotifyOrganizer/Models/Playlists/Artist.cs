using System.Text.Json.Serialization;
using SpotifyOrganizer.Models.General;

namespace SpotifyOrganizer.Models.Playlists
{
    public class Artist
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("href")] public string? Href { get; set; }
        [JsonPropertyName("uri")] public string? Uri { get; set; }
        [JsonPropertyName("external_urls")] public ExternalUrls? ExternalUrls { get; set; }
    }
}

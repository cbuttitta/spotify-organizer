using System.Text.Json.Serialization;

namespace SpotifyOrganizer.Models.Artists
{
    public class ArtistInfo
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("genres")] public List<string>? Genres { get; set; }
    }
}
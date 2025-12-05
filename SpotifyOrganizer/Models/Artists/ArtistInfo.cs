using System.Text.Json.Serialization;

namespace SpotifyOrganizer.Models.Artist
{
    public class ArtistInfo
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("genres")] public List<string> Genres { get; set; }
    }
}
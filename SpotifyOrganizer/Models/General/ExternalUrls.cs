using System.Text.Json.Serialization;

namespace SpotifyOrganizer.Models.General
{
    public class ExternalUrls
    {
        [JsonPropertyName("spotify")] public string Spotify { get; set; }
    }
}
